// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Prompts;
using Common.Types.PromptSets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class PromptRepository(ApplicationDbContext context, PromptMapService mapService, ILogger<PromptRepository> logger) : IPromptRepository
{
    public async Task<IActionResult?> GetPrompts(Guid teamId, Guid prompSetId, int pageIndex, int pageSize, int totalPages)
    {
        int totalRecordCount;
        List<PromptDatabaseType> prompts = await context.Prompts
            .Where(x =>
                x.TeamId == teamId &&
                x.PrompSetId == prompSetId).Include(x => x.MeetingPromptResponses)
            .OrderBy(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(x => x.Order)
            .ToListAsync();

        List<PromptResponseType?> responsePrompts = mapService.ConvertToResponse(prompts);
        totalRecordCount = await context.Prompts.Where(x =>
            x.TeamId == teamId &&
            x.PrompSetId == prompSetId).CountAsync();
        totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
        return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<PromptResponseType?>(responsePrompts, pageIndex, totalPages, totalRecordCount));
    }

    public async Task<IActionResult?> GetPrompt(Guid teamId, Guid prompSetId, Guid promptId)
    {
        PromptDatabaseType? dbPrompt = await context.Prompts.FirstOrDefaultAsync(
            x => x.TeamId == teamId
                 && x.PrompSetId == prompSetId
                 && x.Id == promptId);
        if (dbPrompt != null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPrompt));
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Prompt not found" });
    }


    public async Task<IActionResult?> CreatePrompt(Guid teamId, Guid prompSetId, PromptRequestType? requestPrompt)
    {
        PromptSetDatabaseType? promptSet = await context.PromptSets.Include(x => x.Prompts)
            .FirstOrDefaultAsync(x => x.TeamId == teamId && x.Id == prompSetId);
        if (promptSet != null)
        {
            PromptDatabaseType? dbPrompt = mapService.ConvertToDatabase(requestPrompt);
            try
            {
                dbPrompt!.TeamId = teamId;

                // Find the highest order in the existing prompts
                int? maxOrder = promptSet.Prompts?.Any() == true
                    ? promptSet.Prompts.Max(p => p.Order)
                    : -1;

                // Set the new prompt's order to maxOrder + 1
                dbPrompt.Order = maxOrder + 1;

                promptSet.Prompts?.Add(dbPrompt);
                await context.SaveChangesAsync();
                var prompts = await context.Prompts
                    .Where(x => x.TeamId == teamId && x.PrompSetId == prompSetId)
                    .OrderBy(x => x.Order)
                    .ToListAsync();
                await ReorderPromptsSequentially(prompts);
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPrompt));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, logger);
            }
        }
        else
        {
            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "PromptSet not found" });
        }
    }


    public async Task<IActionResult?> UpdatePrompt(Guid teamId, Guid prompSetId, Guid promptId, PromptRequestType prompt)
    {
        PromptDatabaseType? dbPrompt = await context.Prompts.FirstOrDefaultAsync(x =>
            x.Id == promptId &&
            x.TeamId == teamId &&
            x.PrompSetId == prompSetId
        );
        if (dbPrompt != null)
        {
            try
            {
                mapService.MergeToDatabase(prompt, dbPrompt);
                await context.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbPrompt));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Prompt not found" });
    }

    public async Task<IActionResult?> DeletePrompt(Guid teamId, Guid prompSetId, Guid promptId)
    {
        PromptDatabaseType? dbPrompt = await context.Prompts.FirstOrDefaultAsync(x =>
            x.Id == promptId &&
            x.TeamId == teamId &&
            x.PrompSetId == prompSetId
        );
        if (dbPrompt != null)
        {
            try
            {
                context.Prompts.Remove(dbPrompt);
                await context.SaveChangesAsync();
                var prompts = await context.Prompts
                    .Where(x => x.TeamId == teamId && x.PrompSetId == prompSetId)
                    .OrderBy(x => x.Order)
                    .ToListAsync();
                await ReorderPromptsSequentially(prompts);
                return ApiActions.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Prompt not found" });
    }

    public async Task<IActionResult?> MovePromptUp(Guid teamId, Guid prompSetId, Guid promptId)
    {
        // Find the current prompt
        PromptDatabaseType? currentPrompt = await context.Prompts.FirstOrDefaultAsync(x =>
            x.Id == promptId &&
            x.TeamId == teamId &&
            x.PrompSetId == prompSetId
        );

        if (currentPrompt == null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Prompt not found" });
        }

        // If Order is null, assign a default value
        if (!currentPrompt.Order.HasValue)
        {
            currentPrompt.Order = 0;
        }

        // Find the prompt with the next lower order (the one above the current prompt)
        PromptDatabaseType? promptAbove = await context.Prompts
            .Where(x =>
                x.TeamId == teamId &&
                x.PrompSetId == prompSetId &&
                x.Order.HasValue &&
                x.Order.Value < currentPrompt.Order.Value)
            .OrderByDescending(x => x.Order)
            .FirstOrDefaultAsync();

        // If there's no prompt above (current prompt is already at the top), return a message
        if (promptAbove == null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.OK, new { message = "Prompt is already at the top" });
        }

        try
        {
            // Swap the order values
            if (promptAbove.Order != null)
            {
                int tempOrder = promptAbove.Order.Value;
                promptAbove.Order = currentPrompt.Order;
                currentPrompt.Order = tempOrder;
                await context.SaveChangesAsync();
            }

            var prompts = await context.Prompts
                .Where(x => x.TeamId == teamId && x.PrompSetId == prompSetId)
                .OrderBy(x => x.Order)
                .ToListAsync();
            await ReorderPromptsSequentially(prompts);
            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(currentPrompt));
        }
        catch (Exception exception)
        {
            return ApiActions.HandleApiReturnException(exception, logger);
        }
    }

    public async Task<IActionResult?> MovePromptDown(Guid teamId, Guid prompSetId, Guid promptId)
    {
        // Find the current prompt
        PromptDatabaseType? currentPrompt = await context.Prompts.FirstOrDefaultAsync(x =>
            x.Id == promptId &&
            x.TeamId == teamId &&
            x.PrompSetId == prompSetId
        );

        if (currentPrompt == null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Prompt not found" });
        }

        // Find the prompt with the next higher order (the one below the current prompt)
        PromptDatabaseType? promptBelow = await context.Prompts
            .Where(x =>
                x.TeamId == teamId &&
                x.PrompSetId == prompSetId &&
                x.Order.HasValue &&
                currentPrompt.Order != null &&
                x.Order.Value > currentPrompt.Order.Value)
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync();

        // If there's no prompt below (current prompt is already at the bottom), return a message
        if (promptBelow == null)
        {
            return ApiActions.CreateResponse(HttpStatusCode.OK, new { message = "Prompt is already at the bottom" });
        }

        try
        {
            if (promptBelow.Order != null)
            {
                int tempOrder = promptBelow.Order.Value;
                promptBelow.Order = currentPrompt.Order;
                currentPrompt.Order = tempOrder;
            }

            await context.SaveChangesAsync();

            if (promptBelow.Order == currentPrompt.Order)
            {
                currentPrompt.Order += 1;
            }

            var prompts = await context.Prompts
                .Where(x => x.TeamId == teamId && x.PrompSetId == prompSetId)
                .OrderBy(x => x.Order)
                .ToListAsync();
            await ReorderPromptsSequentially(prompts);
            return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(currentPrompt));
        }
        catch (Exception exception)
        {
            return ApiActions.HandleApiReturnException(exception, logger);
        }
    }

    public async Task ReorderPromptsSequentially(IList<PromptDatabaseType>? prompts)
    {
        if (prompts == null || !prompts.Any())
        {
            return;
        }

        for (int i = 0; i < prompts.Count; i++)
        {
            prompts[i].Order = i + 1;
        }

        await context.SaveChangesAsync();
    }
}