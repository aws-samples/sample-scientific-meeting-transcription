// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockRuntime;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Common;
using Common.AWSServices;
using Common.Types.Meetings;
using Common.Types.StepFunction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Double;

namespace StepFunctionLambda.Services;

public class TranscribeService(
    IAmazonS3? s3Client,
    ILogger<TranscribeService> logger,
    IAmazonSimpleSystemsManagement ssmService,
    AmazonBedrockRuntimeClient bedrockRuntimeClient,
    IAmazonS3 s3Service,
    ApplicationDbContext dbContext) : ITranscribeService
{
    
    public async Task<object> ProcessTranscribeRequest(TranscriptionMeetingInputType input)
    {
        logger.LogInformation("Data received: {@input}", JsonSerializer.Serialize(input));

        var dbMeeting = await dbContext.Meetings
            .AsNoTracking()
            .Include(x => x.PromptSet)
            .FirstAsync(x => x.Id == input.Id);
        logger.LogInformation("Current Step: {@input}", input.CurrentStep.ToString());

        switch (input.CurrentStep)
        {
            case CurrentStepEnum.TranscribeFailed:
                dbMeeting.CurrentStep = CurrentStepEnum.TranscribeFailed;
                dbMeeting.TranscribeError = input?.TranscribeError?.Cause;
                await dbContext.SaveChangesAsync();
                break;
            case CurrentStepEnum.Transcribed:
                try
                {
                    logger.LogInformation("Starting transcribing notes");

                    var request = new GetObjectRequest
                    {
                        BucketName = input.TranscriptionJob?.OutputBucketName,
                        Key = input.TranscriptionJob?.OutputKey,
                    };
                    string transribedJson;
                    logger.LogInformation("Getting S3 Object:  {@input}", input.TranscriptionJob?.OutputBucketName);
                    // Issue request and remember to dispose of the response
                    using (GetObjectResponse response = await s3Client!.GetObjectAsync(request))
                    {
                        using (var reader = new StreamReader(response.ResponseStream))
                        {
                            transribedJson = await reader.ReadToEndAsync();
                        }
                    }

                    TranscribeOutputType? meetingOutputObject =
                        JsonSerializer.Deserialize<TranscribeOutputType>(transribedJson, new JsonSerializerOptions()
                        {
                            NumberHandling = JsonNumberHandling.AllowReadingFromString
                        });

                    var audioSegments = meetingOutputObject?.results?.audio_segments;
                    var stringBuilder = new StringBuilder();
                    MeetingNotesType meetingNotes = new MeetingNotesType
                    {
                        Transcript = meetingOutputObject?.results?.transcripts?[0].transcript,
                        JobName = meetingOutputObject?.jobName,
                        Segments = [],
                        Version = 1,
                        LastEditedBy = "System",
                        LastEditedAt = DateTime.UtcNow.ToString("o"),
                        Words = []
                    };
                    int segmentIndex = 0;
                    string? currentSpeaker = audioSegments?[0].speaker_label;
                    SpeakerSegmentType? currentSegment = null;

                    foreach (var segment in audioSegments!)
                    {
                        if (segment is { start_time: not null, end_time: not null })
                        {
                            if (currentSegment == null || currentSpeaker != segment.speaker_label)
                            {
                                if (currentSegment != null)
                                {
                                    meetingNotes.Segments.Add(currentSegment);
                                    segmentIndex++;
                                }

                                currentSegment = new SpeakerSegmentType
                                {
                                    SegmentId = segmentIndex,
                                    SpeakerLabel = segment.speaker_label,
                                    StartTime = Parse(segment.start_time),
                                    EndTime = Parse(segment.end_time),
                                    Transcript = segment.transcript,
                                };
                                currentSpeaker = segment.speaker_label;
                            }
                            else
                            {
                                currentSegment.EndTime = Parse(segment.end_time);
                                currentSegment.Transcript += " " + segment.transcript;
                            }

                            stringBuilder.AppendLine(
                                $"Speaker: {segment.speaker_label} ({segment.start_time} - {segment.end_time}");
                            stringBuilder.AppendLine(segment.transcript);

                            if (segment.items == null) continue;

                            foreach (var word_index in segment.items)
                            {
                                SpeakerWordType speakerWord = new SpeakerWordType();
                                speakerWord.SegmentId = segmentIndex;
                                ResultItemsType? retrievedItem =
                                    (meetingOutputObject?.results?.items!).FirstOrDefault(x => x?.id == word_index);
                                if (retrievedItem == null) continue;

                                if (retrievedItem.start_time == null && retrievedItem.end_time == null &&
                                    retrievedItem.type == "punctuation")
                                {
                                    speakerWord.WordId = retrievedItem.id;
                                    speakerWord.WordType = "punctuation";
                                    speakerWord.Content = retrievedItem.alternatives?[0].content;
                                }
                                else
                                {
                                    speakerWord.WordId = retrievedItem.id;
                                    speakerWord.WordType = retrievedItem?.type;
                                    speakerWord.Confidence = retrievedItem?.alternatives?[0].confidence;
                                    if (retrievedItem?.end_time != null)
                                        speakerWord.EndTime = Parse(retrievedItem.end_time);
                                    if (retrievedItem?.start_time != null)
                                        speakerWord.StartTime = Parse(retrievedItem.start_time);
                                    speakerWord.Content = retrievedItem?.alternatives?[0].content;
                                }

                                meetingNotes.Words.Add(speakerWord);
                            }
                        }

                        stringBuilder.AppendLine("\n\n");
                    }

                    if (currentSegment != null)
                    {
                        meetingNotes.Segments.Add(currentSegment);
                    }


                    logger.LogInformation("Uploading to S3");
                    var ssmResponse = await ssmService.GetParameterAsync(new GetParameterRequest
                    {
                        Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                        WithDecryption = true
                    });
                    var buketName = ssmResponse.Parameter.Value;
                    var key = dbMeeting.MeetingNotesVersionLocation;
                    PutObjectResponse s3Response = await s3Service.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = buketName,
                        Key = key,
                        ContentBody = JsonSerializer.Serialize(meetingNotes)
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

                    dbMeeting.MeetingNotes = stringBuilder.ToString();


                    if (dbMeeting.Description == String.Empty)
                    {
                        BedrockClaudeClient internalBedrockclient = new BedrockClaudeClient(logger);
                        string systemPrompt =
                            "Generate a 30 and only 30 words at maximum words summary of this meeting notes which will be used in as a description for the meeting. Don't include any no other comments, explanations, reasoning or dialogue";
                        dbMeeting.Description = await internalBedrockclient.Invoke(
                            stringBuilder.ToString(),
                            systemPrompt,
                            bedrockRuntimeClient
                        );
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error: {@e}", e);
                    dbContext.Entry(dbMeeting).State = EntityState.Modified;
                    dbMeeting.CurrentStep = CurrentStepEnum.TranscribeFailed;
                    dbMeeting.TranscribeError = e.Message;
                    await dbContext.SaveChangesAsync();
                    throw;
                }

                dbMeeting.CurrentStep = CurrentStepEnum.Transcribed;
                dbMeeting.MeetingNotesVersion = 1;
                dbMeeting.TranscribeError = "";
                dbContext.Entry(dbMeeting).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                break;
            default:
                dbMeeting.CurrentStep = CurrentStepEnum.TranscribeFailed;
                dbMeeting.TranscribeError = $"Unknown phase: {input.CurrentStep} defined in the workflow";
                await dbContext.SaveChangesAsync();
                throw new Exception($"Unknown phase: {input.CurrentStep}");
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
}