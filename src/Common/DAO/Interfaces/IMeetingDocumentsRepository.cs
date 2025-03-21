// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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