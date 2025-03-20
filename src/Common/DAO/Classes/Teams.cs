// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.DAO.Classes;

public class TeamRepository(ApplicationDbContext context, TeamMapService mapService, ILogger<TeamRepository> logger) : ITeamRepository
{
    public async Task<IActionResult?> GetTeams(int pageIndex, int pageSize, int totalPages)
    {
        try
        {
            List<TeamDatabaseType> teams = await context.Teams
                .OrderBy(b => b.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<TeamResponseType?> responseTeams = mapService.ConvertToResponse(teams);
            var totalRecordCount = await context.Teams.CountAsync();
            totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
            return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<TeamResponseType?>(responseTeams, pageIndex, totalPages, totalRecordCount));
        }
        catch (Exception exception)
        {
            return ApiActions.HandleApiReturnException(exception, logger);
        }
    }

    public async Task<IActionResult?> GetTeam(Guid teamId)
    {
        try
        {
            TeamDatabaseType? dbTeam = await context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
            if (dbTeam != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbTeam));
            }
        }
        catch (Exception exception)
        {
            ApiActions.HandleApiReturnException(exception, logger);
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> CreateTeam(TeamRequestType? team)
    {
        TeamDatabaseType? dbTeam = mapService.ConvertToDatabase(team);
        try
        {
            if (dbTeam != null)
            {
                context.Teams.Add(dbTeam);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception exception)
        {
            ApiActions.HandleApiReturnException(exception, logger);
        }

        return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbTeam));
    }

    public async Task<IActionResult?> UpdateTeam(Guid teamId, TeamRequestType team)
    {
        TeamDatabaseType? dbTeam = await context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
        if (dbTeam != null)
        {
            try
            {
                mapService.MergeToDatabase(team, dbTeam);
                await context.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbTeam));
            }
            catch (Exception exception)
            {
                ApiActions.HandleApiReturnException(exception, logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }

    public async Task<IActionResult?> DeleteTeam(Guid teamId)
    {
        TeamDatabaseType? dbTeam = await context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
        if (dbTeam != null)
        {
            try
            {
                context.Teams.Remove(dbTeam);
                await context.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception exception)
            {
                ApiActions.HandleApiReturnException(exception, logger);
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Team not found" });
    }
}