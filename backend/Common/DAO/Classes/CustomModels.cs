// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.CustomModels;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class CustomModelRepository(ApplicationDbContext context, CustomModelMapService mapService, ILogger<CustomModelRepository> logger) : ICustomModelRepository
{
    public async Task<IActionResult?> GetCustomModels(Guid teamId, int pageIndex, int pageSize, int totalPages)
    {
        List<CustomModelDatabaseType> customModels = await context.CustomModels
            .Where(x => x.TeamId == teamId)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<CustomModelResponseType?> responseCustomModels = mapService.ConvertToResponse(customModels);
        var totalRecordCount = await context.CustomModels.Where(x => x.TeamId == teamId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<CustomModelResponseType?>(responseCustomModels, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetCustomModel(Guid teamId, Guid customModelId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomModels).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomModelDatabaseType? dbCustomModel = team.CustomModels?.FirstOrDefault(x => x.Id == customModelId);
            if (dbCustomModel != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomModel));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "CustomModel not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }

    public async Task<IActionResult?> CreateCustomModel(Guid teamId, CustomModelRequestType? customModel)
    {
        try
        {
            TeamDatabaseType? team = await context.Teams.Include(x => x.CustomModels).FirstOrDefaultAsync(x => x.Id == teamId);
            if (team != null && customModel != null)
            {
                try
                {
                    CustomModelDatabaseType? dbCustomModel = mapService.ConvertToDatabase(customModel);
                    if (dbCustomModel != null)
                    {
                        team.CustomModels?.Add(dbCustomModel);
                        await context.SaveChangesAsync();
                        return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomModel));
                    }
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
        }
        catch (Exception e)
        {
            return ApiActions.HandleApiReturnException(e, logger);
        }
    }

    public async Task<CustomModelDatabaseType?> InternalUpdateCustomModelAsync(Guid teamId, Guid customModelId, CustomModelDatabaseType customModel)
    {
        CustomModelDatabaseType? dbCustomModel = context.CustomModels.ToList().FirstOrDefault(x => x.Id == customModelId && x.TeamId == teamId);
        if (dbCustomModel == null)
        {
            dbCustomModel = mapService.Merge(customModel, dbCustomModel);
            await context.SaveChangesAsync();
            return dbCustomModel;
        }
        else
        {
            throw new Exception("Custom model not found");
        }
    }

    public async Task<IActionResult?> UpdateCustomModel(Guid teamId, Guid customModelId, CustomModelRequestType customModel)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomModels).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomModelDatabaseType? dbCustomModel = team.CustomModels?.FirstOrDefault(x => x.Id == customModelId);
            if (dbCustomModel != null)
            {
                try
                {
                    if (!dbCustomModel.CanPerformValue.Contains(CustomModelCanPerformEnum.Edit))
                    {
                        return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This custom model cannot be updated as this time" });
                    }

                    mapService.MergeToDatabase(customModel, dbCustomModel);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbCustomModel));
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "CustomModel not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }


    public async Task<IActionResult?> DeleteCustomModel(Guid teamId, Guid customModelId, IAmazonTranscribeService transcribeService)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomModels).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomModelDatabaseType? dbCustomModel = team.CustomModels?.FirstOrDefault(x => x.Id == customModelId);
            if (dbCustomModel != null)
            {
                if (!dbCustomModel.CanPerformValue.Contains(CustomModelCanPerformEnum.Delete))
                {
                    return ApiActions.CreateResponse(HttpStatusCode.Forbidden, new { message = "This custom model cannot be deleted as this time" });
                }

                try
                {
                    ListLanguageModelsResponse customModelsResults = await transcribeService.ListLanguageModelsAsync(new ListLanguageModelsRequest());
                    if (customModelsResults.Models.Any(x => x.ModelName == dbCustomModel.Id.ToString()))
                    {
                        if (customModelsResults.Models.First(x => x.ModelName == dbCustomModel.Id.ToString()).ModelStatus == ModelStatus.COMPLETED)
                        {
                            DeleteLanguageModelResponse result = await transcribeService.DeleteLanguageModelAsync(new DeleteLanguageModelRequest { ModelName = dbCustomModel.Id.ToString() });
                            if (result.HttpStatusCode != HttpStatusCode.OK)
                            {
                                return ApiActions.CreateResponse(HttpStatusCode.UnprocessableEntity, $"Can't delete Transcribe Model {result.ResponseMetadata}");
                            }
                        }
                        else
                        {
                            return ApiActions.CreateResponse(HttpStatusCode.UnprocessableEntity, $"Can't delete Transcribe Model because it is not ready");
                        }
                    }

                    context.CustomModels.Remove(dbCustomModel);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception e)
                {
                    return ApiActions.HandleApiReturnException(e, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "CustomModel not found" });
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Something went wrong" });
    }
}