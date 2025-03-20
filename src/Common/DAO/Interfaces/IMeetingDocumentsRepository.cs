// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Common.Types.MeetingDocuments;

namespace Common.DAO.Interfaces
{
    public interface IMeetingDocumentsRepository
    {
        Task<IActionResult?> GetMeetingDocuments(Guid teamId, Guid meetingId, int pageIndex, int pageSize, int totalPages);
        Task<IActionResult?> GetMeetingDocument(Guid teamId, Guid meetingId, Guid documentId);
        Task<IActionResult?> CreateMeetingDocument(Guid teamId, Guid meetingId, MeetingDocumentRequestType? document);
        Task<IActionResult?> UpdateMeetingDocument(Guid teamId, Guid meetingId, Guid documentId, MeetingDocumentRequestType document);
        Task<IActionResult?> DeleteMeetingDocument(Guid teamId, Guid meetingId, Guid documentId);
        Task<IActionResult?> GetMeetingDocumentUrl(Guid teamId, Guid meetingId, Guid documentId);
    }
}