// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.BedrockAgent;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Common;
using Common.AWSServices;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.Meetings;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExscriboAPI.Controllers
{
    [Produces("application/json")]
    [Route("/teams")]
    public class MeetingsController : Controller
    {
        private readonly IMeetingRepository? _meetingRepository;
        private readonly IAmazonSimpleSystemsManagement _ssm;
        private readonly IAmazonS3 _s3;
        private readonly IAmazonStepFunctions _stepFunctions;
        private readonly ApplicationDbContext _dbContext;
        private readonly MeetingMapService _mapService;
        private readonly IAmazonBedrockAgent _amazonBedrockAgent;
        private readonly ILogger _logger;
        private readonly IAmazonSimpleSystemsManagement _ssmService;
        private readonly IAmazonS3 _s3Service;


        public MeetingsController(IMeetingRepository meetingRepository, MeetingMapService mapService,
            ApplicationDbContext dbContext, IAmazonSimpleSystemsManagement ssm, IAmazonStepFunctions stepFunctions, IAmazonS3 s3,
            IAmazonBedrockAgent amazonBedrockAgent, ILoggerFactory loggerFactory,
            IAmazonSimpleSystemsManagement ssmService, IAmazonS3 s3Service)
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _ssmService = ssmService ?? throw new ArgumentNullException(nameof(ssmService));
            _meetingRepository = meetingRepository ?? throw new ArgumentNullException(nameof(meetingRepository));
            _ssm = ssm ?? throw new ArgumentNullException(nameof(ssm));
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _stepFunctions = stepFunctions ?? throw new ArgumentNullException(nameof(stepFunctions));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _logger = loggerFactory.CreateLogger<MeetingsController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _amazonBedrockAgent = amazonBedrockAgent ?? throw new ArgumentNullException(nameof(amazonBedrockAgent));
        }

        [Route("{teamsId:Guid}/meetings/{meetingsId:Guid}/prompt_responses")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<MeetingResponseType>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetMeetingPromptResponses(Guid teamsId, Guid meetingsId, int pageIndex = 1, int pageSize = 10, int totalPages = 1)
        {
            return await _meetingRepository!.GetMeetingPromptResponses(teamsId, meetingsId, pageIndex, pageSize, totalPages);
        }

        [Route("{teamsId:Guid}/meetings")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<MeetingResponseType>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetAllMeetings(Guid teamsId, int pageIndex = 1,
            int pageSize = 10, int totalPages = 1)
        {
            return await _meetingRepository!.GetMeetings(teamsId, pageIndex, pageSize, totalPages);
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:Guid}")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetMeeting(Guid teamsId, Guid meetingsId)
        {
            return await _meetingRepository!.GetMeeting(teamsId, meetingsId);
        }

        [Route("{teamsId:guid}/meetings")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromBody] MeetingRequestType meeting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            return await _meetingRepository!.CreateMeeting(teamsId, meeting);
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/create_signed_url")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> CreateSignedUrl([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            try
            {
                MeetingDatabaseType? dbMeetingRecord = await _dbContext.Meetings.FirstOrDefaultAsync(x => x.Id == meetingsId && x.TeamId == teamsId);
                if (dbMeetingRecord == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Meeting not found");
                }

                var ssmResponse = await _ssm.GetParameterAsync(new GetParameterRequest
                {
                    Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                    WithDecryption = true
                });
                var buketName = ssmResponse.Parameter.Value;
                var uploadfullpath = $"s3://{buketName}/teams/{teamsId}/meetings/{meetingsId}/recording/{meetingsId}.mp4";
                var outputfullpath = $"s3://{buketName}/teams/{teamsId}/meetings/{meetingsId}/transcribed";
                var uploadKey = $"teams/{teamsId}/meetings/{meetingsId}/recording/{meetingsId}.mp4";
                var outputKey = $"teams/{teamsId}/meetings/{meetingsId}/transcribe_job_output.json";


                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = $"{buketName}",
                    Key = uploadKey,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    ContentType = "video/mp4",
                    Protocol = Protocol.HTTPS
                };
                dbMeetingRecord.S3RecordingFullPath = uploadfullpath;
                dbMeetingRecord.PreSignedUrl = await _s3.GetPreSignedURLAsync(urlRequest);
                dbMeetingRecord.S3TranscribedFullPath = outputfullpath;
                dbMeetingRecord.S3OutputBucketName = buketName;
                dbMeetingRecord.S3OutputKeyName = outputKey;
                await _dbContext.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeetingRecord));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/start_prompt_processing")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> StartPromptProcessing([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            string? promptStateMachineArn = EnvironmentHelper.PROMPT_STATEMACHINE_ARN;

            try
            {
                MeetingDatabaseType? dbMeetingRecord = await _dbContext.Meetings
                    .FirstOrDefaultAsync(x => x.Id == meetingsId && x.TeamId == teamsId);
                if (dbMeetingRecord == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Meeting not found");
                }

                StepFunctionCombinedInputType inputObject = new StepFunctionCombinedInputType()
                {
                    StepFunctionJobType = StepFunctionJobType.PromptProcess,
                    PromptProcessInput = new PromptProcessLambdaInputType()
                    {
                        MeetingId = meetingsId
                    }
                };
                //clear existing prompt responses
                StartExecutionRequest executionRequest = new StartExecutionRequest
                {
                    Name = Guid.NewGuid().ToString(),
                    Input = JsonSerializer.Serialize(inputObject),
                    StateMachineArn = promptStateMachineArn
                };

                // response from the StartExecution service method, as returned by StepFunction.
                var response = await _stepFunctions.StartExecutionAsync(executionRequest);
                dbMeetingRecord.TranscribeError = "";
                dbMeetingRecord.CurrentStep = CurrentStepEnum.PromptProcessing;
                dbMeetingRecord.StateMachineExecutionArn = response.ExecutionArn;

                await _dbContext.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeetingRecord));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/meeting_notes_urls")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> DownloadMeetingNotes([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            try
            {
                MeetingDatabaseType? dbMeetingRecord = await _dbContext.Meetings
                    .FirstOrDefaultAsync(x => x.Id == meetingsId && x.TeamId == teamsId);
                if (dbMeetingRecord == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Meeting not found");
                }

                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = dbMeetingRecord.S3OutputBucketName,
                    Key = dbMeetingRecord.MeetingNotesVersionLocation,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    ContentType = "application/json",
                    Protocol = Protocol.HTTPS
                };
                var urlUpdate = new GetPreSignedUrlRequest
                {
                    BucketName = dbMeetingRecord.S3OutputBucketName,
                    Key = dbMeetingRecord.MeetingNotesVersionLocationUpdate,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    ContentType = "application/json",
                    Protocol = Protocol.HTTPS
                };
                var urlRecording = new GetPreSignedUrlRequest
                {
                    BucketName = dbMeetingRecord.S3OutputBucketName,
                    Key = dbMeetingRecord.MeetingRecordingLocation,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    ContentType = "video/mp4",
                    Protocol = Protocol.HTTPS
                };
                return ApiActions.CreateResponse(HttpStatusCode.OK, new
                    {
                        download_link = await _s3.GetPreSignedURLAsync(urlRequest),
                        upload_link = await _s3.GetPreSignedURLAsync(urlUpdate),
                        recording_link = await _s3.GetPreSignedURLAsync(urlRecording)
                    }
                );
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/seal_meeting")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> SealMeeting([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            return await _meetingRepository!.SealMeeting(teamsId, meetingsId);
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/start_transcription_processing")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> StartTranscriptionProcessing([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            try
            {
                MeetingDatabaseType? dbMeetingRecord = await _dbContext.Meetings
                    .Include(x => x.CustomModel)
                    .Include(x => x.PromptSet).ThenInclude(p => p!.Prompts)
                    .FirstOrDefaultAsync(x => x.Id == meetingsId && x.TeamId == teamsId);
                if (dbMeetingRecord == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Meeting not found");
                }

                StepFunctionCombinedInputType inputObject = new StepFunctionCombinedInputType()
                {
                    StepFunctionJobType = StepFunctionJobType.Transcribe,
                    TranscribeInput = new TranscriptionMeetingInputType()
                    {
                        Id = dbMeetingRecord.Id,
                        TranscriptionJob = new TranscriptionJobInputType()
                        {
                            MediaFileUri = dbMeetingRecord.S3RecordingFullPath,
                            LanguageCode = dbMeetingRecord.CustomModel?.LanguageCode ?? "en-US",
                            TranscriptionJobName = dbMeetingRecord.Id.ToString(),
                            OutputBucketName = dbMeetingRecord.S3OutputBucketName,
                            OutputKey = dbMeetingRecord.S3OutputKeyName,
                            VocabularyName = dbMeetingRecord.CustomVocabularyId?.ToString()
                        }
                    }
                };

                StartExecutionRequest executionRequest = new StartExecutionRequest
                {
                    Name = Guid.NewGuid().ToString(),
                    Input = JsonSerializer.Serialize(inputObject),
                    StateMachineArn = EnvironmentHelper.TRANSCRIBE_STATEMACHINE_ARN
                };

                // response from the StartExecution service method, as returned by StepFunction.
                var response = await _stepFunctions.StartExecutionAsync(executionRequest);
                dbMeetingRecord.TranscribeError = "";
                dbMeetingRecord.CurrentStep = CurrentStepEnum.Transcribing;
                dbMeetingRecord.StateMachineExecutionArn = response.ExecutionArn;

                await _dbContext.SaveChangesAsync();
                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbMeetingRecord));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeetingResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId, [FromBody] MeetingRequestType meeting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            return await _meetingRepository!.UpdateMeeting(teamsId, meetingsId, meeting);
        }


        [Route("{teamsId:guid}/meetings/{meetingsId:guid}/meeting_analytics")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> DownloadAnalyticsDocument([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            return await _meetingRepository!.DownloadAnalyticsDocument(teamsId, meetingsId);
        }

        [Route("{teamsId:guid}/meetings/{meetingsId:guid}")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid meetingsId)
        {
            MeetingDatabaseType? dbMeetingRecord = await _dbContext.Meetings
                .Include(x => x.MeetingDocuments)
                .FirstOrDefaultAsync(x => x.Id == meetingsId && x.TeamId == teamsId);
            if (dbMeetingRecord == null)
            {
                return ApiActions.CreateResponse(HttpStatusCode.NotFound, "Meeting not found");
            }

            try
            {
                if (dbMeetingRecord.MeetingDocuments != null)
                {
                    foreach (var document in dbMeetingRecord.MeetingDocuments)
                    {
                        await BedrockKnowledgeBase.DeleteKnowledgeBaseObject(_amazonBedrockAgent, document.Id.ToString(), _logger);
                    }
                }

                await BedrockKnowledgeBase.DeleteKnowledgeBaseObject(_amazonBedrockAgent, meetingsId.ToString(), _logger);

                var objectsDelete = new List<KeyVersion>();
                //delete S3 objects
                var ssmResponse = await _ssmService.GetParameterAsync(new GetParameterRequest
                {
                    Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                    WithDecryption = true
                });
                var objectList = await _s3Service.ListObjectsV2Async(new ListObjectsV2Request()
                {
                    BucketName = ssmResponse.Parameter.Value,
                    Prefix = $"teams/{teamsId}/meetings/{meetingsId}"
                });
                if (objectList.S3Objects != null)
                {
                    objectsDelete.AddRange(objectList.S3Objects.Select(x => new KeyVersion()
                    {
                        Key = x.Key
                    }));
                    if (dbMeetingRecord.IncludeInModelTraining == true)
                    {
                        objectsDelete.Add(new KeyVersion()
                        {
                            Key = $"teams/{dbMeetingRecord.TeamId}/custommodels/{dbMeetingRecord.CustomModelId}/trainingdata/meeting_notes_{dbMeetingRecord.Id}.txt"
                        });
                    }

                    _logger.LogInformation("Deleting objects {@objectsDelete}", objectsDelete);
                    DeleteObjectsResponse s3Response = await _s3Service.DeleteObjectsAsync(new DeleteObjectsRequest()
                    {
                        BucketName = ssmResponse.Parameter.Value,
                        Objects = objectsDelete
                    });
                    _logger.LogInformation("S3 delete response {@s3Response}", s3Response);
                }

                return await _meetingRepository!.DeleteMeeting(teamsId, meetingsId);
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }
    }
}