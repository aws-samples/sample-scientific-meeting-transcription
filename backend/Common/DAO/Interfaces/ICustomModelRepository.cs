// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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