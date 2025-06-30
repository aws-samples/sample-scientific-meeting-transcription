// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Common.AWSServices;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Bedrock;
using Common.Types.Prompts;
using Common.Types.PromptSets;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class PromptSetRepository(
    ApplicationDbContext context,
    PromptSetMapService mapService,
    ILogger<PromptSetRepository> logger,
    AmazonBedrockRuntimeClient bedrockRuntimeClient)
    : IPromptSetRepository
{
    public async Task<IActionResult?> GetPromptSets(Guid teamsId, int pageIndex,
        int pageSize, int totalPages)
    {
        List<PromptSetDatabaseType> promptSets = await context.PromptSets
            .Where(x => x.TeamId == teamsId)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<PromptSetResponseType?> responsePromptSets = mapService.ConvertToResponse(promptSets);
        var totalRecordCount = await context.PromptSets.Where(x => x.TeamId == teamsId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK,
            new PaginatedList<PromptSetResponseType?>(responsePromptSets, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetPromptSet(Guid teamsId, Guid promptSetId)
    {
        PromptSetDatabaseType? dbPromptSet = await context.PromptSets.FirstOrDefaultAsync(x =>
            x.Id == promptSetId &&
            x.TeamId == teamsId);
        if (dbPromptSet != null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPromptSet));
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "PromptSet not found" });
    }

    public async Task<IActionResult?> CreatePromptSet(Guid teamId, PromptSetRequestType? promptSet)
    {
        TeamDatabaseType? team = await context.Teams.Include(teamDatabaseType => teamDatabaseType.PromptSets)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            if (promptSet != null)
            {
                PromptSetDatabaseType? dbPromptSet = mapService.ConvertToDatabase(promptSet);
                if (dbPromptSet != null)
                {
                    try
                    {
                        team.PromptSets?.Add(dbPromptSet);
                        dbPromptSet.Prompts = new List<PromptDatabaseType>();
                        if ((bool)dbPromptSet.CreatePromptsFromDescription! && dbPromptSet.Description != null)
                        {
                            string? result = await BedrockProcessor.ProcessRequest(
                                dbPromptSet.Description, null,
                                logger,
                                bedrockRuntimeClient,
                                "Generate a list of 5 LLM prompts that will extract the information outlined in this description. Output the list in a simple json array with only the prompt without any use of variables." +
                                "use the following output template: " +
                                "[\n " +
                                "prompt1" +
                                "prompt2" +
                                "\n]"
                            );
                            if (result != null)
                            {
                                var outputJson = JsonExtractor.NaiveJsonFromText(result);
                                int index = 1;
                                outputJson?.RootElement.EnumerateArray().ToList().ForEach(x =>
                                {
                                    PromptDatabaseType prompt = new PromptDatabaseType
                                    {
                                        Order = index,
                                        TeamId = team.Id,
                                        Prompt = x.GetString()
                                    };
                                    dbPromptSet.Prompts?.Add(prompt);
                                    index += 1;
                                });
                            }
                        }

                        await context.SaveChangesAsync();

                        return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPromptSet));
                    }
                    catch (Exception exception)
                    {
                        return ApiActions.HandleApiReturnException(exception, logger);
                    }
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "PromptSet not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> UpdatePromptSet(Guid teamsId, Guid promptSetId,
        PromptSetRequestType promptSet)
    {
        TeamDatabaseType? team = await context.Teams.Include(teamDatabaseType => teamDatabaseType.PromptSets)
            .FirstOrDefaultAsync(x => x.Id == teamsId);
        if (team != null)
        {
            PromptSetDatabaseType? dbPromptSet = team.PromptSets?.FirstOrDefault(x => x.Id == promptSetId);
            if (dbPromptSet != null)
            {
                try
                {
                    mapService.MergeToDatabase(promptSet, dbPromptSet);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPromptSet));
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "PromptSet not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> DeletePromptSet(Guid teamId, Guid promptSetId)
    {
        TeamDatabaseType? team = await context.Teams.Include(teamDatabaseType => teamDatabaseType.PromptSets)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            PromptSetDatabaseType? dbPromptSet = team.PromptSets?.FirstOrDefault(x => x.Id == promptSetId);
            if (dbPromptSet != null)
            {
                try
                {
                    context.PromptSets.Remove(dbPromptSet);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "PromptSet not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }
}