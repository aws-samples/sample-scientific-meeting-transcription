// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Common.AWSServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.MeetingDocuments;
using Common.Types.Meetings;
using Common.Utilities;
using DocumentIdentifier = Amazon.BedrockAgent.Model.DocumentIdentifier;
using DocumentStatus = Amazon.BedrockAgent.DocumentStatus;
using S3Location = Amazon.BedrockAgent.Model.S3Location;


namespace Common.DAO.Classes
{
    public class MeetingDocumentsRepository(
        ApplicationDbContext context,
        MeetingDocumentMapService mapService,
        IAmazonSimpleSystemsManagement ssmService,
        IAmazonS3 s3Service,
        ILogger<MeetingDocumentsRepository> logger)
        : IMeetingDocumentsRepository
    {
        public async Task<IActionResult?> GetMeetingDocuments(Guid teamId, Guid meetingId, int pageIndex, int pageSize, int totalPages)
        {
            List<MeetingDocumentDatabaseType> meetingDocuments = await context.MeetingDocuments
                .Where(x =>
                    x.TeamId == teamId &&
                    x.MeetingId == meetingId)
                .OrderBy(b => b.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<MeetingDocumentResponseType?> responseMeetingDocuments = mapService.ConvertToResponse(meetingDocuments);
            int totalRecordCount = await context.MeetingDocuments.Where(x =>
                x.TeamId == teamId &&
                x.MeetingId == meetingId).CountAsync();
            totalPages = (int)Math.Ceiling(totalRecordCount / (double)pageSize);
            return ApiActions.CreateResponse(HttpStatusCode.OK, new PaginatedList<MeetingDocumentResponseType?>(responseMeetingDocuments, pageIndex, totalPages, totalRecordCount));
        }

        public async Task<IActionResult?> GetMeetingDocument(Guid teamId, Guid meetingId, Guid meetingDocumentId)
        {
            MeetingDocumentDatabaseType? dbMeetingDocument = await context.MeetingDocuments.FirstOrDefaultAsync(
                x => x.TeamId == teamId
                     && x.MeetingId == meetingId
                     && x.Id == meetingDocumentId);
            if (dbMeetingDocument != null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbMeetingDocument));
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "MeetingDocument not found" });
        }


        public async Task<IActionResult?> CreateMeetingDocument(Guid teamId, Guid meetingId, MeetingDocumentRequestType? requestMeetingDocument)
        {
            MeetingDatabaseType? meeting = await context.Meetings
                .Include(x => x.MeetingDocuments)
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.Id == meetingId);
            if (meeting != null)
            {
                if (requestMeetingDocument == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.BadRequest, new { message = "payload is empty" });
                }

                MeetingDocumentDatabaseType? dbMeetingDocument = mapService.ConvertToDatabase(requestMeetingDocument);
                try
                {
                    dbMeetingDocument!.TeamId = teamId;
                    meeting.MeetingDocuments?.Add(dbMeetingDocument);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbMeetingDocument));
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting not found" });
        }


        public async Task<IActionResult?> UpdateMeetingDocument(Guid teamId, Guid meetingId, Guid meetingDocumentId, MeetingDocumentRequestType meetingDocument)
        {
            MeetingDocumentDatabaseType? dbMeetingDocument = await context.MeetingDocuments.FirstOrDefaultAsync(x =>
                x.Id == meetingDocumentId &&
                x.TeamId == teamId &&
                x.MeetingId == meetingId
            );
            if (dbMeetingDocument != null)
            {
                try
                {
                    mapService.MergeToDatabase(meetingDocument, dbMeetingDocument);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.OK, mapService.ConvertToResponse(dbMeetingDocument));
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting document not found" });
        }

        public async Task<IActionResult?> DeleteMeetingDocument(Guid teamId, Guid meetingId, Guid meetingDocumentId)
        {
            MeetingDocumentDatabaseType? dbMeetingDocument = await context.MeetingDocuments.FirstOrDefaultAsync(x =>
                x.Id == meetingDocumentId &&
                x.TeamId == teamId &&
                x.MeetingId == meetingId
            );
            if (dbMeetingDocument != null)
            {
                try
                {
                    var ssmResponse = await ssmService.GetParameterAsync(new GetParameterRequest
                    {
                        Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                        WithDecryption = true
                    });

                    DeleteObjectResponse s3Response = await s3Service.DeleteObjectAsync(new DeleteObjectRequest()
                    {
                        BucketName = ssmResponse.Parameter.Value,
                        Key = dbMeetingDocument.DocumenLocation
                    });
                    logger.LogInformation("S3 delete response {@s3Response}", s3Response);

                    context.MeetingDocuments.Remove(dbMeetingDocument);
                    await context.SaveChangesAsync();
                    return ApiActions.CreateResponse(HttpStatusCode.NoContent);
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting document not found" });
        }

        public async Task<IActionResult?> GetMeetingDocumentUrl(Guid teamId, Guid meetingId, Guid meetingDocumentId)
        {
            MeetingDocumentDatabaseType? dbMeetingDocument = await context.MeetingDocuments.FirstOrDefaultAsync(x =>
                x.Id == meetingDocumentId &&
                x.TeamId == teamId &&
                x.MeetingId == meetingId
            );
            if (dbMeetingDocument != null)
            {
                try
                {
                    var ssmResponse = await ssmService.GetParameterAsync(new GetParameterRequest
                    {
                        Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                        WithDecryption = true
                    });
                    var urlRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = ssmResponse.Parameter.Value,
                        Key = dbMeetingDocument.DocumenLocation,
                        Verb = HttpVerb.PUT,
                        Expires = DateTime.UtcNow.AddMinutes(10),
                        ContentType = TextUtilities.GetMimeType(dbMeetingDocument.Filename),
                        Protocol = Protocol.HTTPS
                    };
                    string singedUrl = await s3Service.GetPreSignedURLAsync(urlRequest);
                    return ApiActions.CreateResponse(HttpStatusCode.OK, new { url = singedUrl });
                }
                catch (Exception exception)
                {
                    return ApiActions.HandleApiReturnException(exception, logger);
                }
            }

            return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Meeting document not found" });
        }
    }
}