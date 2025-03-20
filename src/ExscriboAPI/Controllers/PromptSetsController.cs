// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.PromptSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers;

[Produces("application/json")]
[Route("/teams")]
public class PromptSetsController(IPromptSetRepository? promptSetRepository) : Controller
{
    private readonly IPromptSetRepository? _promptSetRepository = promptSetRepository ?? throw new ArgumentNullException(nameof(promptSetRepository));

    [Route("{teamsId:Guid}/promptSets")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<PromptSetResponseType>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetAllPromptSets(Guid teamsId, int pageIndex = 1,
        int pageSize = 10, int totalPages = 1)
    {
        return await _promptSetRepository!.GetPromptSets(teamsId, pageIndex, pageSize, totalPages);
    }

    [Route("{teamsId:Guid}/promptSets/{promptSetsId:Guid}")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromptSetResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetPromptSet(Guid teamsId, Guid promptSetsId)
    {
        return await _promptSetRepository!.GetPromptSet(teamsId, promptSetsId);
    }

    [Route("{teamsId:Guid}/promptSets")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PromptSetResponseType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromBody] PromptSetRequestType promptSet)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _promptSetRepository!.CreatePromptSet(teamsId, promptSet);
    }

    [Route("{teamsId:Guid}/promptSets/{promptSetsId:guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromptSetResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromBody] PromptSetRequestType promptSet)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _promptSetRepository!.UpdatePromptSet(teamsId, promptSetsId, promptSet);
    }

    [Route("{teamsId:Guid}/promptSets/{promptSetsId:guid}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId)
    {
        return await _promptSetRepository!.DeletePromptSet(teamsId, promptSetsId);
    }
}