// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers
{
    /// <summary>
    /// Controller for managing teams within the application
    /// Provides endpoints for CRUD operations on team resources
    /// </summary>
    [Produces("application/json")]
    [Route("/teams")]
    public class TeamsController(ITeamRepository teamRepository) : Controller
    {
        private readonly ITeamRepository _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));

        /// <summary>
        /// Retrieves a paginated list of all teams
        /// </summary>
        /// <param name="pageIndex">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="totalPages">The total number of pages</param>
        /// <returns>A paginated list of teams</returns>
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

        /// <summary>
        /// Retrieves a specific team by its ID
        /// </summary>
        /// <param name="teamsId">The unique identifier of the team</param>
        /// <returns>The requested team if found</returns>
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

        /// <summary>
        /// Creates a new team
        /// </summary>
        /// <param name="team">The team data to create</param>
        /// <returns>The newly created team</returns>
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

        /// <summary>
        /// Updates an existing team
        /// </summary>
        /// <param name="teamsId">The unique identifier of the team to update</param>
        /// <param name="team">The updated team data</param>
        /// <returns>The updated team</returns>
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

        /// <summary>
        /// Deletes a team by its ID
        /// </summary>
        /// <param name="teamsId">The unique identifier of the team to delete</param>
        /// <returns>No content if successful</returns>
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