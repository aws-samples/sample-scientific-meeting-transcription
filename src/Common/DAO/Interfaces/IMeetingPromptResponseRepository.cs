// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.MeetingPromptResponses;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface IMeetingPromptResponseRepository
{
    Task<IActionResult?> GetMeetingPromptResponses(Guid teamId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId);
    Task<IActionResult?> CreateMeetingPromptResponse(Guid teamId, MeetingPromptResponseRequestType? meetingPromptResponse);
    Task<IActionResult?> UpdateMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId, MeetingPromptResponseRequestType meetingPromptResponse);
    Task<IActionResult?> DeleteMeetingPromptResponse(Guid teamId, Guid meetingPromptResponseId);
}