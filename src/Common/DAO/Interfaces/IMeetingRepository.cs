// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Common.Types.Meetings;
using Microsoft.AspNetCore.Mvc;

namespace Common.DAO.Interfaces;

public interface IMeetingRepository
{
    Task<IActionResult?> GetMeetings(Guid teamId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetMeetingPromptResponses(Guid teamId, Guid meetingId, int pageIndex, int pageSize, int totalPages);
    Task<IActionResult?> GetMeeting(Guid teamId, Guid meetingId);
    Task<IActionResult?> CreateMeeting(Guid teamId, MeetingRequestType? meeting);
    Task<IActionResult?> UpdateMeeting(Guid teamId, Guid meetingId, MeetingRequestType meeting);
    Task<IActionResult?> DeleteMeeting(Guid teamId, Guid meetingId);
    Task<IActionResult?> SealMeeting(Guid teamId, Guid meetingId);
    Task<IActionResult?> DownloadAnalyticsDocument(Guid teamId, Guid meetingId);
}