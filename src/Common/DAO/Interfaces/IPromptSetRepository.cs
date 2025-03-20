// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.PromptSets;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface IPromptSetRepository
{
    Task<IActionResult?> GetPromptSets(Guid teamId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetPromptSet(Guid teamId, Guid promptSetId);
    Task<IActionResult?> CreatePromptSet(Guid teamId, PromptSetRequestType? promptSet);
    Task<IActionResult?> UpdatePromptSet(Guid teamId, Guid promptSetId, PromptSetRequestType promptSet);
    Task<IActionResult?> DeletePromptSet(Guid teamId, Guid promptSetId);
}