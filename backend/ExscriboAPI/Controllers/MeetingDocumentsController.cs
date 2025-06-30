// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.MeetingDocuments;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers;

[Produces("application/json")]
[Route("/teams")]
public class MeetingDocumentsController(
    IMeetingDocumentsRepository? meetingDocumentRepository,
    ApplicationDbContext dbContext) : Controller
{
    private readonly IMeetingDocumentsRepository? _meetingDocumentRepository = meetingDocumentRepository ?? throw new ArgumentNullException(nameof(meetingDocumentRepository));
    private readonly ApplicationDbContext? _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    [Route("{teamsId:Guid}/meetings/{meetingsId}/documents")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<MeetingDocumentResponseType>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetAllMeetingDocuments(Guid teamsId, Guid meetingsId, int pageIndex = 1,
        int pageSize = 10, int totalPages = 1)
    {
        return await _meetingDocumentRepository!.GetMeetingDocuments(teamsId, meetingsId, pageIndex, pageSize, totalPages);
    }

    [Route("{teamsId:Guid}/meetings/{meetingsId}/documents/{meetingDocumentsId:Guid}")]
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeetingDocumentResponseType))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetMeetingDocument(Guid teamsId, Guid meetingsId, Guid meetingDocumentsId)
    {
        return await _meetingDocumentRepository!.GetMeetingDocument(teamsId, meetingsId, meetingDocumentsId);
    }

    [Route("{teamsId:Guid}/meetings/{meetingsId}/documents")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingDocumentResponseType))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId, [FromBody] MeetingDocumentRequestType meetingDocument)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return await _meetingDocumentRepository!.CreateMeetingDocument(teamsId, meetingsId, meetingDocument);
    }

    [Route("{teamsId:Guid}/meetings/{meetingsId}/documents/{meetingDocumentsId:guid}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId, [FromRoute] Guid meetingDocumentsId)
    {
        return await _meetingDocumentRepository!.DeleteMeetingDocument(teamsId, meetingsId, meetingDocumentsId);
    }

    [Route("{teamsId:Guid}/meetings/{meetingsId}/documents/{meetingDocumentsId:guid}/document_upload_url")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult?> GetUploadUrl([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId, [FromRoute] Guid meetingDocumentsId)
    {
        return await _meetingDocumentRepository!.GetMeetingDocumentUrl(teamsId, meetingsId, meetingDocumentsId);
    }
}