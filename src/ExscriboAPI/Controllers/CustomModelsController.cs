// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.TranscribeService;
using Common;
using Common.AWSServices;
using Common.DAO.Interfaces;
using Common.Types;
using Common.Types.CustomModels;
using Common.Types.StepFunction;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace ExscriboAPI.Controllers
{
    [Produces("application/json")]
    [Route("/teams")]
    public class CustomModelsController : Controller
    {
        private readonly ICustomModelRepository? _customModelRepository;
        private readonly IAmazonSimpleSystemsManagement _ssm;
        private readonly IAmazonS3 _s3;
        private readonly ApplicationDbContext _dbContext;
        private readonly CustomModelMapService _mapService;
        private readonly IAmazonStepFunctions _stepFunctions;
        private readonly IAmazonTranscribeService _transcribeService;
        private readonly ILogger<CustomModelsController> _logger;

        public CustomModelsController(ICustomModelRepository? customModelRepository, IAmazonTranscribeService transcribeService, IAmazonStepFunctions stepFunctions, IAmazonSimpleSystemsManagement ssm,
            IAmazonS3 s3,
            ApplicationDbContext dbContext, CustomModelMapService mapService, ILogger<CustomModelsController> logger)
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            _customModelRepository = customModelRepository ?? throw new ArgumentNullException(nameof(customModelRepository));
            _ssm = ssm ?? throw new ArgumentNullException(nameof(ssm));
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _stepFunctions = stepFunctions ?? throw new ArgumentNullException(nameof(stepFunctions));
            _transcribeService = transcribeService ?? throw new ArgumentNullException(nameof(transcribeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [Route("{teamsId:Guid}/customModels")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<CustomModelResponseType>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetAllCustomModels(Guid teamsId, int pageIndex = 1,
            int pageSize = 10, int totalPages = 1)
        {
            if (_customModelRepository == null)
            {
                throw new InvalidOperationException("CustomModelRepository is not initialized");
            }

            return await _customModelRepository.GetCustomModels(teamsId, pageIndex, pageSize, totalPages);
        }

        [Route("{teamsId:guid}/customModels/{customModelsId:Guid}")]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomModelResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> GetCustomModel(Guid teamsId, Guid customModelsId)
        {
            if (_customModelRepository == null)
            {
                throw new InvalidOperationException("CustomModelRepository is not initialized");
            }

            return await _customModelRepository.GetCustomModel(teamsId, customModelsId);
        }

        [Route("{teamsId:guid}/customModels")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CustomModelResponseType))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Create([FromRoute] Guid teamsId, [FromBody] CustomModelRequestType customModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (_customModelRepository == null)
            {
                throw new InvalidOperationException("CustomModelRepository is not initialized");
            }

            return await _customModelRepository.CreateCustomModel(teamsId, customModel);
        }

        [Route("{teamsId:guid}/customModels/{customModelsId:guid}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomModelResponseType))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Update([FromRoute] Guid teamsId, [FromRoute] Guid customModelsId, [FromBody] CustomModelRequestType customModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (_customModelRepository == null)
            {
                throw new InvalidOperationException("CustomModelRepository is not initialized");
            }

            return await _customModelRepository.UpdateCustomModel(teamsId, customModelsId, customModel);
        }

        [Route("{teamsId:guid}/customModels/{customModelsId:guid}")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> Delete([FromRoute] Guid teamsId, [FromRoute] Guid customModelsId)
        {
            if (_customModelRepository == null)
            {
                throw new InvalidOperationException("CustomModelRepository is not initialized");
            }

            return await _customModelRepository.DeleteCustomModel(teamsId, customModelsId, _transcribeService);
        }

        [Route("{teamsId:guid}/customModels/{customModelsId:guid}/create_signed_url")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> CreateSignedUrl([FromRoute] Guid teamsId, [FromRoute] Guid customModelsId)
        {
            try
            {
                var ssmResponse = await _ssm.GetParameterAsync(new GetParameterRequest
                {
                    Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                    WithDecryption = true
                });
                var buketName = ssmResponse.Parameter.Value;
                var key = $"teams/{teamsId}/custommodels/{customModelsId}/trainingdata/trainingdata.txt";
                var uploadfullpath = $"s3://{buketName}/{key}";
                var trainingfullpath = $"s3://{buketName}/teams/{teamsId}/custommodels/{customModelsId}/trainingdata";

                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = buketName,
                    Key = key,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    ContentType = "text/plain",
                    Protocol = Protocol.HTTPS
                };
                CustomModelDatabaseType? dbCustomModel = null;
                TeamDatabaseType? team = await _dbContext.Teams.Include(x => x.CustomModels).FirstOrDefaultAsync(x => x.Id == teamsId);
                if (team != null)
                {
                    dbCustomModel = team?.CustomModels?.FirstOrDefault(x => x.Id == customModelsId);
                    if (dbCustomModel != null)
                    {
                        string presignedUrl = _s3.GetPreSignedURL(urlRequest);
                        dbCustomModel.ModelSetupProgress = CustomModelSetupProgressEnum.S3SignedUrlCreated;
                        dbCustomModel.TrainingDataS3Uri = uploadfullpath;
                        dbCustomModel.TrainingDataS3UriFolder = trainingfullpath;
                        dbCustomModel.PreSignedUrl = presignedUrl;
                        dbCustomModel.PreSignedUrlExpire = urlRequest.Expires;
                        await _dbContext.SaveChangesAsync();
                    }
                }

                if (dbCustomModel == null)
                {
                    return ApiActions.CreateResponse(HttpStatusCode.NotFound);
                }

                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbCustomModel));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }

        [Route("{teamsId:guid}/customModels/{customModelsId:guid}/start_model_training")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> StartModelTraining([FromRoute] Guid teamsId, [FromRoute] Guid customModelsId)
        {
            try
            {
                _logger.LogInformation($"StartModelTraining Debug: {teamsId} {customModelsId}", teamsId, customModelsId);

                CustomModelDatabaseType? dbCustomModel = _dbContext.CustomModels.FirstOrDefault(x => x.Id == customModelsId && x.TeamId == teamsId);
                _logger.LogInformation($"dbCustomModel {dbCustomModel}", dbCustomModel);

                StartExecutionRequest executionRequest = new StartExecutionRequest();
                executionRequest.StateMachineArn = EnvironmentHelper.CUSTOMMODEL_STATEMACHINE_ARN;
                executionRequest.Input = JsonSerializer.Serialize(new StepFunctionCombinedInputType()
                {
                    StepFunctionJobType = StepFunctionJobType.CustomModel,
                    CustomModelInput = new CustomModelStepMachineType()
                    {
                        CustomModel = _mapService.ConvertToResponse(dbCustomModel)
                    }
                }, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Converters = { new JsonStringEnumConverter() }
                });
                executionRequest.Name = $"CustomModel_Training_{customModelsId}_{ShortGuid.Generate()}";

                _logger.LogInformation($"executionRequest {executionRequest}", executionRequest);

                var response = await _stepFunctions.StartExecutionAsync(executionRequest);
                if (dbCustomModel != null)
                {
                    dbCustomModel.ModelSetupProgress = CustomModelSetupProgressEnum.TrainingStarted;
                    dbCustomModel.StateMachineExecutionArn = response.ExecutionArn;
                    await _dbContext.SaveChangesAsync();
                }

                return ApiActions.CreateResponse(HttpStatusCode.OK, _mapService.ConvertToResponse(dbCustomModel));
            }
            catch (Exception exception)
            {
                return ApiActions.HandleApiReturnException(exception, _logger);
            }
        }
    }
}