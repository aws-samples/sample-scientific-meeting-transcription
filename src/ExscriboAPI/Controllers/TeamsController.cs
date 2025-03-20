// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Teams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers
{
    [Produces("application/json")]
    [Route("/teams")]
    public class TeamsController(ITeamRepository teamRepository) : Controller
    {
        private readonly ITeamRepository _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));

        [Route("")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<TeamResponseType>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetAllTeams(
            int pageIndex = 1,
            int pageSize = 10,
            int totalPages = 1)
        {
            return await _teamRepository.GetTeams(pageIndex, pageSize, totalPages);
        }

        [Route("{teamsId:Guid}")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetTeam(Guid teamsId)
        {
            return await _teamRepository.GetTeam(teamsId);
        }

        [Route("")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TeamResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Create([FromBody] TeamRequestType team)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            return await _teamRepository.CreateTeam(team);
        }

        [Route("{teamsId:Guid}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromBody] TeamRequestType team)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            return await _teamRepository.UpdateTeam(teamsId, team);
        }

        [Route("{teamsId:Guid}")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Delete([FromRoute] Guid teamsId)
        {
            return await _teamRepository.DeleteTeam(teamsId);
        }
    }
}