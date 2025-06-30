// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.CustomVocabularies;
using Common.Types.Teams;
using Common.Types.VocabularyPhrases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class VocabularyPhraseRepository(ApplicationDbContext context, VocabularyPhraseMapService mapService, ILogger<VocabularyPhraseRepository> logger) : IVocabularyPhraseRepository
{
    public async Task<IActionResult?> GetVocabularyPhrases(Guid teamId, Guid customVocabularyId, int pageIndex, int pageSize, int totalPages)
    {
        List<VocabularyPhraseDatabaseType> vocabularyPhrases = await context.VocabularyPhrases
            .Where(x => x.TeamId == teamId && x.CustomVocabularyId == customVocabularyId)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<VocabularyPhraseResponseType?> responseVocabularyPhrases = mapService.ConvertToResponse(vocabularyPhrases);
        var totalRecordCount = await context.VocabularyPhrases.Where(x => x.TeamId == teamId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<VocabularyPhraseResponseType?>(responseVocabularyPhrases, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId)
    {
        TeamDatabaseType? team = await context.Teams.Include(x => x.CustomVocabularies).FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            VocabularyPhraseDatabaseType? dbVocabularyPhrase =  await context.VocabularyPhrases
                .FirstOrDefaultAsync(x => x.Id == vocabularyPhraseId
                                     && x.CustomVocabularyId == vocabularyPhraseId
                                     && x.TeamId == teamId);
            if (dbVocabularyPhrase != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbVocabularyPhrase));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "VocabularyPhrase not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }

    public async Task<IActionResult?> CreateVocabularyPhrase(Guid teamId, Guid customVocabularyId, VocabularyPhraseRequestType? vocabularyPhrase)
    {
        try
        {
            TeamDatabaseType? team = await context.Teams
                .Include(x => x.CustomVocabularies)!
                .ThenInclude(x => x.Phrases)
                .FirstOrDefaultAsync(x => x.Id == teamId);
            if (team != null && vocabularyPhrase != null)
            {
                CustomVocabularyDatabaseType? customVocabulary = team.CustomVocabularies?.FirstOrDefault(x => x.Id == customVocabularyId);
                try
                {
                    var dbVocabularyPhrase = mapService.ConvertToDatabase(vocabularyPhrase);
                    if (dbVocabularyPhrase != null)
                    {
                        dbVocabularyPhrase.TeamId = teamId;
                        customVocabulary?.Phrases?.Add(dbVocabularyPhrase);
                        if (customVocabulary != null) customVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.Created;
                        await context.SaveChangesAsync();
                        return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbVocabularyPhrase));
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

    public async Task<IActionResult?> UpdateVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId, VocabularyPhraseRequestType vocabularyPhrase)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.CustomVocabularies)!
            .ThenInclude(x => x.Phrases)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            if (team.CustomVocabularies != null)
            {
                CustomVocabularyDatabaseType? dbcustomVocabulary = team.CustomVocabularies.FirstOrDefault(x => x.Id == customVocabularyId);
                if (dbcustomVocabulary != null)
                {
                    VocabularyPhraseDatabaseType? dbVocabularyPhrase = dbcustomVocabulary.Phrases?.FirstOrDefault(x => x.Id == vocabularyPhraseId);
                    try
                    {
                        if (dbVocabularyPhrase != null)
                        {
                            mapService.MergeToDatabase(vocabularyPhrase, dbVocabularyPhrase);
                            dbcustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.Created;
                            await context.SaveChangesAsync();
                            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbVocabularyPhrase));
                        }
                    }
                    catch (Exception e)
                    {
                        return ApiActions.HandleApiReturnException(e, logger);
                    }

                    return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Vocabulary Phrase not found");
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Custom Vocabulary not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }


    public async Task<IActionResult?> DeleteVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId)
    {
        TeamDatabaseType? team = await context.Teams
            .Include(x => x.CustomVocabularies)!
            .ThenInclude(x => x.Phrases)
            .FirstOrDefaultAsync(x => x.Id == teamId);
        if (team != null)
        {
            CustomVocabularyDatabaseType? dbcustomVocabulary = team.CustomVocabularies?.FirstOrDefault(x => x.Id == customVocabularyId);
            if (dbcustomVocabulary != null)
            {
                VocabularyPhraseDatabaseType? dbVocabularyPhrase = dbcustomVocabulary.Phrases?.FirstOrDefault(x => x.Id == vocabularyPhraseId);
                if (dbVocabularyPhrase != null)
                {
                    try
                    {
                        dbcustomVocabulary.Phrases?.Remove(dbVocabularyPhrase);
                        dbcustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.Created;
                        await context.SaveChangesAsync();
                        return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                    }
                    catch (Exception e)
                    {
                        return ApiActions.HandleApiReturnException(e, logger);
                    }
                }

                return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Vocabulary Phrase not found");
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Custom Vocabulary not found");
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Team not found");
    }
}