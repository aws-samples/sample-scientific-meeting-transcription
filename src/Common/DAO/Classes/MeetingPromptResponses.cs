// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.MeetingPromptResponses;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class MeetingPromptResponseRepository(ApplicationDbContext context, MeetingPromptResponseMapService mapService, ILogger<MeetingPromptResponseRepository> logger)
    : IMeetingPromptResponseRepository
{
    public async Task<IActionResult?> GetMeetingPromptResponses(Guid teamId, int pageIndex, int pageSize, int totalPages)
    {
        List<MeetingPromptResponseDatabaseType> meetingPromptResponses = await context.MeetingPromptResponses
            .Where(x => x.TeamId == teamId)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<MeetingPromptResponseResponseType?> responseMeetingPromptResponses = mapService.ConvertToResponse(meetingPromptResponses);
        var totalRecordCount = await context.MeetingPromptResponses.Where(x => x.TeamId == teamId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<MeetingPromptResponseResponseType?>(responseMeetingPromptResponses, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.MeetingPromptResponses).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingPromptResponseDatabaseType? dbMeetingPromptResponse = team.MeetingPromptResponses?.FirstOrDefault(x => x.Id == meetingPromptResponseId);
            if (dbMeetingPromptResponse != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.Accepted, mapService.ConvertToResponse(dbMeetingPromptResponse));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "MeetingPromptResponse not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> CreateMeetingPromptResponse(Guid teamId, MeetingPromptResponseRequestType? meetingPromptResponse)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.MeetingPromptResponses).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null && meetingPromptResponse != null)
        {
            MeetingPromptResponseDatabaseType? dbMeetingPromptResponse;
            try
            {
                dbMeetingPromptResponse = mapService.ConvertToDatabase(meetingPromptResponse);
                if (dbMeetingPromptResponse != null)
                {
                    team.MeetingPromptResponses?.Add(dbMeetingPromptResponse);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbMeetingPromptResponse));
                }
            }
            catch (Exception e)
            {
                return ApiActions.HandleApiReturnException(e, logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> UpdateMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId, MeetingPromptResponseRequestType meetingPromptResponse)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.MeetingPromptResponses).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingPromptResponseDatabaseType? dbMeetingPromptResponse = await context.MeetingPromptResponses.FirstOrDefaultAsync(x => x.Id == meetingPromptResponseId && x.TeamId == teamId);
            if (dbMeetingPromptResponse != null)
            {
                try
                {
                    mapService.MergeToDatabase(meetingPromptResponse, dbMeetingPromptResponse);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbMeetingPromptResponse));
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "MeetingPromptResponse not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not Found" });
    }

    public async Task<IActionResult?> DeleteMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.MeetingPromptResponses).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            MeetingPromptResponseDatabaseType? dbMeetingPromptResponse = team.MeetingPromptResponses?.FirstOrDefault(x => x.Id == meetingPromptResponseId);
            if (dbMeetingPromptResponse != null)
            {
                try
                {
                    context.MeetingPromptResponses.Remove(dbMeetingPromptResponse);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "MeetingPromptResponse not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }
}