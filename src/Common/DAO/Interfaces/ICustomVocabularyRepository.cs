// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.CustomVocabularies;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface ICustomVocabularyRepository
{
    Task<IActionResult?> GetCustomVocabularies(Guid teamId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetCustomVocabulary(Guid teamId, Guid customVocabularyId);
    Task<IActionResult?> CreateCustomVocabulary(Guid teamId, CustomVocabularyRequestType? customVocabularyId);
    Task<IActionResult?> UpdateCustomVocabulary(Guid teamId, Guid customVocabularyId, CustomVocabularyRequestType customVocabulary);
    Task<IActionResult?> DeleteCustomVocabulary(Guid teamId, Guid customVocabularyId);
    Task<IActionResult?> PublishCustomVocabulary(Guid teamId, Guid customVocabularyId);
}