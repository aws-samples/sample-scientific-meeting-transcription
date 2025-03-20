// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.Prompts;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface IPromptRepository
{
    Task<IActionResult?> GetPrompts(Guid teamId, Guid prompSetId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetPrompt(Guid teamId, Guid prompSetId, Guid promptId);
    Task<IActionResult?> CreatePrompt(Guid teamId, Guid prompSetId, PromptRequestType? prompt);
    Task<IActionResult?> UpdatePrompt(Guid teamId, Guid prompSetId, Guid promptId, PromptRequestType prompt);
    Task<IActionResult?> DeletePrompt(Guid teamId, Guid prompSetId, Guid promptId);
    Task<IActionResult?> MovePromptUp(Guid teamId, Guid prompSetId, Guid promptId);
    Task<IActionResult?> MovePromptDown(Guid teamId, Guid prompSetId, Guid promptId);
}