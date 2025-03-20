// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.VocabularyPhrases;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface IVocabularyPhraseRepository
{
    Task<IActionResult?> GetVocabularyPhrases(Guid teamId, Guid customVocabularyId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId);
    Task<IActionResult?> CreateVocabularyPhrase(Guid teamId, Guid customVocabularyId, VocabularyPhraseRequestType? vocabularyPhrase);
    Task<IActionResult?> UpdateVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId, VocabularyPhraseRequestType vocabularyPhrase);
    Task<IActionResult?> DeleteVocabularyPhrase(Guid teamId, Guid customVocabularyId, Guid vocabularyPhraseId);
}