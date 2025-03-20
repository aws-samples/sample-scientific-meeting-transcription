// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Common.AWSServices;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.MeetingPromptResponses;
using Common.Types.Meetings;
using Common.Types.StepFunction;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class MeetingRepository(ApplicationDbContext context, IAmazonStepFunctions stepFunctions, MeetingMapService mapService, ILoggerFactory loggerFactory) : IMeetingRepository
{
    private readonly MeetingMapService _mapService = mapService;
    private readonly ILogger _logger = loggerFactory.CreateLogger<MeetingRepository>();

    public async Task<IActionResult?> GetMeetings(Guid teamId, int pageIndex, int pageSize, int totalPages)
    {
        List<MeetingDatabaseType> meetings = await context.Meetings
            .Where(x => x.TeamId == teamId)
            .Include(x => x.PromptSet)
            .Include(x => x.PromptSet!.Prompts)
            .Include(x => x.CustomVocabulary)
            .Include(x => x.MeetingDocuments)
            .Include(x => x.Team)
            .Include(x => x.CustomModel)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<MeetingResponseType?> responseMeetings = _mapService.ConvertToResponse(meetings);
        var totalRecordCount = await context.Meetings.Where(x => x.TeamId == teamId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<MeetingResponseType?>(responseMeetings, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetMeetingPromptResponses(Guid teamId, Guid meetingId, int pageIndex, int pageSize, int totalPages)
    {
        List<MeetingPromptResponseSlimType> promptResponses = await context.MeetingPromptResponses
            .AsNoTracking()
            .Where(x => x.TeamId == teamId && x.MeetingId == meetingId)
            .Select(x => new MeetingPromptResponseSlimType
            {
                Id = x.Id,
                PromptResponse = x.PromptResponse,
                Prompt = x.Prompt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync();
        var totalRecordCount = await context.MeetingPromptResponses.Where(x => x.TeamId == teamId && x.MeetingId == meetingId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<MeetingPromptResponseSlimType>(promptResponses, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetMeeting(Guid teamId, Guid meetingId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.Meetings).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingDatabaseType? dbMeeting = team.Meetings?
                .FirstOrDefault(x => x.Id == meetingId);
            if (dbMeeting != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeeting));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> CreateMeeting(Guid teamId, MeetingRequestType? meeting)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.Meetings).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            try
            {
                MeetingDatabaseType? dbMeeting = _mapService.ConvertToDatabase(meeting);
                if (dbMeeting != null)
                {
                    team.Meetings?.Add(dbMeeting);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeeting));
                }
            }
            catch (Exception e)
            {
                return ApiActions.HandleApiReturnException(e, _logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> UpdateMeeting(Guid teamId, Guid meetingId, MeetingRequestType meeting)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.Meetings).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingDatabaseType? dbMeeting = team.Meetings?.FirstOrDefault(x => x.Id == meetingId);
            if (dbMeeting != null)
            {
                try
                {
                    if (!dbMeeting.CanPerformValue.Contains(CanPerformEnum.Edit))
                    {
                        return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This meeting cannot be edited as this time" });
                    }

                    dbMeeting = _mapService.MergeToDatabase(meeting, dbMeeting);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeeting));
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, _logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> SealMeeting(Guid teamId, Guid meetingId)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.Meetings)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingDatabaseType? dbMeeting = team.Meetings?.FirstOrDefault(x => x.Id == meetingId);
            if (dbMeeting != null)
            {
                if (!dbMeeting.CanPerformValue.Contains(CanPerformEnum.Seal))
                {
                    return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This meeting cannot be sealed as this time" });
                }

                StepFunctionCombinedInputType inputObject = new StepFunctionCombinedInputType()
                {
                    StepFunctionJobType = StepFunctionJobType.SealMeeting,
                    SealMeetingInput = new SealMeetingStepMachineType()
                    {
                        Id = dbMeeting.Id
                    }
                };

                StartExecutionRequest executionRequest = new StartExecutionRequest
                {
                    Name = Guid.NewGuid().ToString(),
                    Input = JsonSerializer.Serialize(inputObject),
                    StateMachineArn = EnvironmentHelper.SEALMEETING_STATEMACHINE_ARN
                };

                // response from the StartExecution service method, as returned by StepFunction.
                await stepFunctions.StartExecutionAsync(executionRequest);
                dbMeeting.TranscribeError = "";
                dbMeeting.CurrentStep = CurrentStepEnum.Sealing;
                await context.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeeting));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> DownloadAnalyticsDocument(Guid teamId, Guid meetingId)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.Meetings)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingDatabaseType? dbMeeting = team.Meetings?.FirstOrDefault(x => x.Id == meetingId);
            if (dbMeeting != null)
            {
                if (!dbMeeting.CanPerformValue.Contains(CanPerformEnum.ViewAnalytics))
                {
                    return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This meeting analytics cannot be downloaded as this time" });
                }

                return ApiActions.CreateResponse(HttpStatusCode.OK, dbMeeting.MeetingAnalyticsPayload?.RootElement.ToString());
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> DeleteMeeting(Guid teamId, Guid meetingId)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.Meetings)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingDatabaseType? dbMeeting = team.Meetings?.FirstOrDefault(x => x.Id == meetingId);
            if (dbMeeting != null)
            {
                try
                {
                    if (!dbMeeting.CanPerformValue.Contains(CanPerformEnum.Delete))
                    {
                        return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This meeting cannot be deleted as this time" });
                    }

                    context.Meetings.Remove(dbMeeting);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, _logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }
}