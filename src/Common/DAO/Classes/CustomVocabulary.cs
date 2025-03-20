// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Common.AWSServices;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.CustomVocabularies;
using Common.Types.StepFunction;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class CustomVocabularyRepository(
    ApplicationDbContext context,
    IAmazonTranscribeService transcribeService,
    CustomVocabularyMapService mapService,
    ILoggerFactory loggerFactory,
    IAmazonStepFunctions stepFunctions) : ICustomVocabularyRepository
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CustomVocabularyRepository>();

    public async Task<IActionResult?> GetCustomVocabularies(Guid teamId, int pageIndex, int pageSize, int totalPages)
    {
        List<CustomVocabularyDatabaseType> customVocabularies = await context.CustomVocabularies
            .Where(x => x.TeamId == teamId)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<CustomVocabularyResponseType?> responseCustomVocabularys = mapService.ConvertToResponse(customVocabularies);
        var totalRecordCount = await context.CustomVocabularies.Where(x => x.TeamId == teamId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<CustomVocabularyResponseType?>(responseCustomVocabularys, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetCustomVocabulary(Guid teamId, Guid customVocabularyId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomVocabularies).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team == null) return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");

        CustomVocabularyDatabaseType? dbCustomVocabulary = team.CustomVocabularies?.FirstOrDefault(x => x.Id == customVocabularyId);
        if (dbCustomVocabulary != null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomVocabulary));
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "CustomVocabulary not found");
    }

    public async Task<IActionResult?> CreateCustomVocabulary(Guid teamId, CustomVocabularyRequestType? customVocabulary)
    {
        try
        {
            TeamDatabaseType? team = await context.Teams.Include(x => x.CustomVocabularies).FirstOrDefaultAsync(x => x.Id == teamId);
            if (team == null || customVocabulary == null) return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
            try
            {
                var dbCustomVocabulary = mapService.ConvertToDatabase(customVocabulary);
                if (dbCustomVocabulary != null)
                {
                    team.CustomVocabularies?.Add(dbCustomVocabulary);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomVocabulary));
                }
            }
            catch (Exception e)
            {
                return ApiActions.HandleApiReturnException(e, _logger);
            }
        }
        catch (Exception e)
        {
            return ApiActions.HandleApiReturnException(e, _logger);
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }


    public async Task<IActionResult?> UpdateCustomVocabulary(Guid teamId, Guid customVocabularyId, CustomVocabularyRequestType customVocabulary)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.CustomVocabularies)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomVocabularyDatabaseType? dbCustomVocabulary = team.CustomVocabularies?.FirstOrDefault(x => x.Id == customVocabularyId);
            if (dbCustomVocabulary != null)
            {
                try
                {
                    if (!dbCustomVocabulary.CanPerformValue.Contains(CustomVocabularyCanPerformEnum.Edit))
                    {
                        return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This custom vocabulary cannot be updated this time" });
                    }

                    mapService.MergeToDatabase(customVocabulary, dbCustomVocabulary);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomVocabulary));
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, _logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Custom Vocabulary not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }


    public async Task<IActionResult?> DeleteCustomVocabulary(Guid teamId, Guid customVocabularyId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomVocabularies).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomVocabularyDatabaseType? dbCustomVocabulary = team.CustomVocabularies?.FirstOrDefault(x => x.Id == customVocabularyId);
            if (dbCustomVocabulary != null)
            {
                try
                {
                    if (!dbCustomVocabulary.CanPerformValue.Contains(CustomVocabularyCanPerformEnum.Delete))
                    {
                        return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This custom vocabulary cannot be deleted this time" });
                    }

                    var listCustomVocabularyResult = await transcribeService.ListVocabulariesAsync(new ListVocabulariesRequest()
                    {
                        NameContains = dbCustomVocabulary.Id.ToString()
                    });
                    if (listCustomVocabularyResult?.Vocabularies?.Count > 0)
                    {
                        await transcribeService.DeleteVocabularyAsync(new DeleteVocabularyRequest()
                        {
                            VocabularyName = dbCustomVocabulary.Id.ToString()
                        });
                    }

                    context.CustomVocabularies.Remove(dbCustomVocabulary);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, _logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Custom Vocabulary not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> PublishCustomVocabulary(Guid teamId, Guid customVocabularyId)
    {
        if (EnvironmentHelper.CUSTOMVOCABULARY_STATEMACHINE_ARN == null)
        {
            throw new Exception("CUSTOMVOCABULARY_STATEMACHINE_ARN environment variable not set");
        }

        var team = await context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            var dbCustomVocabulary = await context.CustomVocabularies
                .Include(x => x.Phrases)
                .FirstOrDefaultAsync(y => y.Id == customVocabularyId);
            if (dbCustomVocabulary != null)
            {
                if (!dbCustomVocabulary.CanPerformValue.Contains(CustomVocabularyCanPerformEnum.Publish))
                {
                    return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This custom vocabulary cannot be published as this time" });
                }

                StartExecutionRequest executionRequest = new StartExecutionRequest();
                executionRequest.StateMachineArn = EnvironmentHelper.CUSTOMVOCABULARY_STATEMACHINE_ARN;
                executionRequest.Input = JsonSerializer.Serialize(new StepFunctionCombinedInputType()
                {
                    StepFunctionJobType = StepFunctionJobType.CustomVocabulary,
                    CustomVocabularyInput = new CustomVocabularyStepMachineType()
                    {
                        Id = dbCustomVocabulary.Id
                    }
                }, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Converters = { new JsonStringEnumConverter() }
                });
                executionRequest.Name = $"CustomVocabulary_{customVocabularyId}_{ShortGuid.Generate()}";

                _logger.LogInformation("Request {@executionRequest}", executionRequest);

                await stepFunctions.StartExecutionAsync(executionRequest);
                dbCustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.Publishing;
                await context.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomVocabulary));
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }
}