// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Prompts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers;

[Produces("application/json")]
[Route("/teams")]
public class PromptsController(IPromptRepository promptRepository) : Controller
{
    private readonly IPromptRepository? _promptRepository = promptRepository ?? throw new ArgumentNullException(nameof(promptRepository));

    [Route("{teamsId:Guid}/promptsets/{promptSetsId:Guid}/prompts")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<PromptResponseType>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetAllPrompts(Guid teamsId, Guid promptSetsId, int pageIndex = 1,
        int pageSize = 10, int totalPages = 1)
    {
        return await _promptRepository!.GetPrompts(teamsId, promptSetsId, pageIndex, pageSize, totalPages);
    }

    [Route("{teamsId:Guid}/promptsets/{promptSetsId:Guid}/prompts/{promptsId:Guid}")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromptResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetPrompt(Guid teamsId, Guid promptSetsId, Guid promptsId)
    {
        return await _promptRepository!.GetPrompt(teamsId, promptSetsId, promptsId);
    }

    [Route("{teamsId:Guid}/promptsets/{promptSetsId:Guid}/prompts")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PromptResponseType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromBody] PromptRequestType prompt)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _promptRepository!.CreatePrompt(teamsId, promptSetsId, prompt);
    }

    [Route("{teamsId:guid}/promptsets/{promptSetsId:Guid}/prompts/{promptsId:Guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromptResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromRoute] Guid promptsId, [FromBody] PromptRequestType prompt)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _promptRepository!.UpdatePrompt(teamsId, promptSetsId, promptsId, prompt);
    }

    [Route("{teamsId:guid}/promptsets/{promptSetsId:Guid}/prompts/{promptsId:Guid}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromRoute] Guid promptsId)
    {
        return await _promptRepository!.DeletePrompt(teamsId, promptSetsId, promptsId);
    }

    [Route("{teamsId:guid}/promptsets/{promptSetsId:Guid}/prompts/{promptsId:Guid}/moveup")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> MovePromptUp([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromRoute] Guid promptsId)
    {
        return await _promptRepository!.MovePromptUp(teamsId, promptSetsId, promptsId);
    }

    [Route("{teamsId:guid}/promptsets/{promptSetsId:Guid}/prompts/{promptsId:Guid}/movedown")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> MovePromptDown([FromRoute] Guid teamsId, [FromRoute] Guid promptSetsId, [FromRoute] Guid promptsId)
    {
        return await _promptRepository!.MovePromptDown(teamsId, promptSetsId, promptsId);
    }
}