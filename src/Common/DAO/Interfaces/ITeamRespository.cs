// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface ITeamRepository
{
    Task<IActionResult?> GetTeams(int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetTeam(Guid teamId);
    Task<IActionResult?> CreateTeam(TeamRequestType? team);
    Task<IActionResult?> UpdateTeam(Guid teamId, TeamRequestType team);
    Task<IActionResult?> DeleteTeam(Guid teamId);
}