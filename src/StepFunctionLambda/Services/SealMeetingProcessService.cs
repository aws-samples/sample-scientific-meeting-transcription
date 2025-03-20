// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Net;
using System.Text;
using Amazon.BedrockAgent;
using Amazon.BedrockRuntime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Common;
using Common.AWSServices;
using Common.Types.Bedrock;
using Common.Types.Meetings;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Internal;

namespace StepFunctionLambda.Services;

public class SealMeetingProcessService(
    ApplicationDbContext dbContext,
    ILogger<SealMeetingProcessService> logger,
    IAmazonSimpleSystemsManagement ssmService,
    IAmazonS3 s3Service,
    AmazonBedrockRuntimeClient bedrockRuntimeClientService,
    IAmazonBedrockAgent amazonBedrockAgent)
    : ISealMeetingProcessService
{
    public async Task<object> SealMeetingService(SealMeetingStepMachineType input)
    {
        MeetingDatabaseType? dbMeetingRecord = await dbContext.Meetings
            .AsNoTracking()
            .Include(meetingDatabaseType => meetingDatabaseType.MeetingDocuments)
            .Include(meetingDatabaseType => meetingDatabaseType.Team)
            .FirstOrDefaultAsync(x => x.Id == input.Id);
        try
        {
            logger.LogInformation("Data received: {@input}", input);
            if (dbMeetingRecord == null)
            {
                throw new Exception("Meeting not found in database");
            }

            string usersPrompt = LlmJsonAnalyticsTemplate.UserPrompt($"<MeetingNotes> \n\n {dbMeetingRecord.MeetingNotes} \n\n</MeetingNotes>");
            string systemPrompt = LlmJsonAnalyticsTemplate.SystemPrompt();

            string? result = await BedrockProcessor.ProcessRequest(
                usersPrompt, dbMeetingRecord.MeetingNotes!,
                logger,
                bedrockRuntimeClientService, systemPrompt);

            if (result == null)
            {
                throw new Exception("Bedrock Processing Failed");
            }

            logger.LogInformation("Bedrock Processing Complete: {@Result}", result);
            dbMeetingRecord.MeetingAnalyticsPayload = JsonExtractor.NaiveJsonFromText(result);

            StringBuilder sealedMeetingNotes = new StringBuilder();
            sealedMeetingNotes.AppendLine(dbMeetingRecord.MeetingAnalyticsPayload?.RootElement.ToJson());
            sealedMeetingNotes.AppendLine(dbMeetingRecord.MeetingNotes);

            logger.LogInformation("Sealed Meeting Contents: {sealedMeetingNotes}", sealedMeetingNotes);

            var attributes = new Dictionary<string, string?>()
            {
                { "TeamId", dbMeetingRecord.TeamId.ToString() },
                { "MeetingId", dbMeetingRecord.Id.ToString() }
            };
            await BedrockKnowledgeBase.IngestTextKnowledgeBaseObject(
                amazonBedrockAgent,
                dbMeetingRecord.Id.ToString(),
                dbMeetingRecord.MeetingNotes!,
                attributes,
                logger
            );
            var s3Bucket = await ssmService.GetParameterAsync(new GetParameterRequest
            {
                Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                WithDecryption = true
            });

            //Ingest documents attached to the meeting
            logger.LogInformation("Ingesting Documents");
            if (dbMeetingRecord.MeetingDocuments != null)
            {
                foreach (var document in dbMeetingRecord.MeetingDocuments)
                {
                    logger.LogInformation("Processing {Filename}", document.Filename);

                    var request = new GetObjectRequest
                    {
                        BucketName = s3Bucket.Parameter.Value,
                        Key = document.DocumenLocation
                    };

                    var memoryStream = new MemoryStream();
                    var response = await s3Service.GetObjectAsync(request);
                    await response.ResponseStream.CopyToAsync(memoryStream);

                    if (document.MimeType != null)
                    {
                        await BedrockKnowledgeBase.IngestDocumentKnowledgeBaseObject(
                            amazonBedrockAgent,
                            $"{document.MeetingId.ToString()}-{document.Id.ToString()}-{document.Filename}",
                            memoryStream,
                            document.MimeType,
                            attributes,
                            logger
                        );
                        logger.LogInformation("Done Processing {Filename}", document.Filename);
                    }

                    memoryStream.Position = 0;
                }
            }

            if (dbMeetingRecord is { IncludeInModelTraining: true, MeetingNotes: not null })
            {
                logger.LogInformation("Uploading to S3");
                PutObjectResponse s3Response = await s3Service.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = s3Bucket.Parameter.Value,
                    Key = $"teams/{dbMeetingRecord.TeamId}/custommodels/{dbMeetingRecord.CustomModelId}/trainingdata/meeting_notes_{dbMeetingRecord.Id}.txt",
                    ContentBody = dbMeetingRecord.MeetingNotes
                });
                logger.LogInformation("Uploaded to S3: {@S3Response}", s3Response);
                if (s3Response.HttpStatusCode == HttpStatusCode.OK)
                {
                    logger.LogInformation("Uploaded to S3: {@S3Response}", s3Response);
                }
                else
                {
                    logger.LogError("S3 Upload Failed: {@S3Response}", s3Response);
                    throw new Exception("S3 Upload Failed");
                }
            }

            dbMeetingRecord.CurrentStep = CurrentStepEnum.Sealed;
            dbMeetingRecord.TranscribeError = "";
            dbContext.Entry(dbMeetingRecord).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Seal Process Failed");
            if (dbMeetingRecord != null)
            {
                dbMeetingRecord.CurrentStep = CurrentStepEnum.SealFailed;
                dbMeetingRecord.TranscribeError = exception.Message;
            }

            await dbContext.SaveChangesAsync();
            throw;
        }
    }
}