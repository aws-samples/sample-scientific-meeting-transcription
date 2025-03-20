// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.VocabularyPhrases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers;

[Produces("application/json")]
[Route("/teams")]
public class VocabularyPhrasesController(IVocabularyPhraseRepository? vocabularyPhrasesRepository) : Controller
{
    private readonly IVocabularyPhraseRepository _vocabularyPhrasesRepository = vocabularyPhrasesRepository ?? throw new ArgumentNullException(nameof(vocabularyPhrasesRepository));

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}/vocabularyphrases")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<VocabularyPhraseResponseType>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetAllvocabularyPhrases(Guid teamsId, Guid customVocabulariesId, int pageIndex = 1, int pageSize = 10, int totalPages = 1)
    {
        return await _vocabularyPhrasesRepository.GetVocabularyPhrases(teamsId, customVocabulariesId, pageIndex, pageSize, totalPages);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}/vocabularyphrases/{vocabularyPhrasesId:Guid}")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VocabularyPhraseResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetVocabularyPhrase(Guid teamsId, Guid customVocabulariesId, Guid vocabularyPhrasesId)
    {
        return await _vocabularyPhrasesRepository.GetVocabularyPhrase(teamsId, customVocabulariesId, vocabularyPhrasesId);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}/vocabularyphrases")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VocabularyPhraseResponseType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId, [FromBody] VocabularyPhraseRequestType vocabularyPhrase)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await vocabularyPhrasesRepository?.CreateVocabularyPhrase(teamsId, customVocabulariesId, vocabularyPhrase)!;
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}/vocabularyphrases/{vocabularyPhrasesId:Guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VocabularyPhraseResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId, [FromRoute] Guid vocabularyPhrasesId,
        [FromBody] VocabularyPhraseRequestType vocabularyPhrase)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _vocabularyPhrasesRepository.UpdateVocabularyPhrase(teamsId, customVocabulariesId, vocabularyPhrasesId, vocabularyPhrase);
    }

    [Route("{teamsId:Guid}/customvocabularies/{customVocabulariesId:Guid}/vocabularyphrases/{vocabularyPhrasesId:Guid}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid customVocabulariesId, [FromRoute] Guid vocabularyPhrasesId)
    {
        return await _vocabularyPhrasesRepository.DeleteVocabularyPhrase(teamsId, customVocabulariesId, vocabularyPhrasesId);
    }
}