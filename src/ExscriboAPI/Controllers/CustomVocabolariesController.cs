// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.CustomVocabularies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers;

[Produces("application/json")]
[Route("/teams")]
public class CustomVocabulariesController : Controller
{
    private readonly ICustomVocabularyRepository? _customVocabularyRepository;

    public CustomVocabulariesController(ICustomVocabularyRepository? customVocabularyRepository)
    {
        _customVocabularyRepository = customVocabularyRepository;
    }

    [Route("{teamsId:Guid}/customvocabularies")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<CustomVocabularyResponseType>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetAllCustomVocabularies(Guid teamsId, int pageIndex = 1,
        int pageSize = 10, int totalPages = 1)
    {
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.GetCustomVocabularies(teamsId, pageIndex, pageSize, totalPages);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomVocabularyResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetCustomVocabulary(Guid teamsId, Guid customVocabulariesId)
    {
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.GetCustomVocabulary(teamsId, customVocabulariesId);
    }

    [Route("{teamsId:Guid}/customvocabularies")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CustomVocabularyResponseType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromBody] CustomVocabularyRequestType customVocabulary)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.CreateCustomVocabulary(teamsId, customVocabulary);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomVocabularyResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId, [FromBody] CustomVocabularyRequestType customVocabulary)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.UpdateCustomVocabulary(teamsId, customVocabulariesId, customVocabulary);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:guid}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId)
    {
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.DeleteCustomVocabulary(teamsId, customVocabulariesId);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:guid}/publish")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Publish([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId)
    {
        if (_customVocabularyRepository == null)
        {
            throw new InvalidOperationException("CustomVocabularyRepository is not initialized");
        }

        return await _customVocabularyRepository.PublishCustomVocabulary(teamsId, customVocabulariesId);
    }
}