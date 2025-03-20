// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Amazon.TranscribeService;
using Common.Types.CustomModels;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface ICustomModelRepository
{
    Task<IActionResult?> GetCustomModels(Guid teamId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetCustomModel(Guid teamId, Guid customModelId);
    Task<IActionResult?> CreateCustomModel(Guid teamId, CustomModelRequestType? customModel);
    Task<IActionResult?> UpdateCustomModel(Guid teamId, Guid customModelId, CustomModelRequestType customModel);
    Task<IActionResult?> DeleteCustomModel(Guid teamId, Guid customModelId, IAmazonTranscribeService transcribeService);
    Task<CustomModelDatabaseType?> InternalUpdateCustomModelAsync(Guid teamId, Guid customModelId, CustomModelDatabaseType customModel);
}