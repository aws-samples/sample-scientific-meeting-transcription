// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StepFunctionLambda.Services;

/// <summary>
/// Configure Lambda serializer to use System.Text.Json for efficient JSON serialization/deserialization
/// </summary>
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace StepFunctionLambda
{
    /// <summary>
    /// Main Lambda entry point class that handles AWS Step Function requests.
    /// Processes different types of jobs based on the StepFunctionJobType in the payload.
    /// This class serves as the primary handler for all Step Function state machine executions,
    /// routing requests to appropriate service implementations based on job type.
    /// </summary>
    public class LambdaEntryPoint
    {
        /// <summary>
        /// Main Lambda handler function that processes Step Function requests.
        /// This method is the entry point for all AWS Lambda invocations from Step Functions.
        /// It configures services, processes the request based on job type, and returns appropriate results.
        /// </summary>
        /// <param name="stepfunctionPayload">Combined input payload from Step Function containing job type and job-specific data</param>
        /// <returns>IActionResult containing the result of the operation (OkObjectResult for success, BadRequestObjectResult for errors)</returns>
        public async Task<IActionResult> FunctionHandlerAsync(StepFunctionCombinedInputType stepfunctionPayload)
        {
            // Set up configuration from environment variables
            // This allows the Lambda to access environment variables set in the Lambda configuration
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            // Set up dependency injection container
            // This creates a service provider with all required services registered in Startup
            var serviceCollection = new ServiceCollection();
            var startup = new Startup(configuration);
            startup.ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Resolve required services from DI container
            // Each service is responsible for a specific job type processing
            ILogger logger = serviceProvider.GetRequiredService<ILogger<LambdaEntryPoint>>();
            ITranscribeService transcribeService = serviceProvider.GetRequiredService<ITranscribeService>();
            IPromptProcessService promptProcessService = serviceProvider.GetRequiredService<IPromptProcessService>();
            ICustomModelTrainingService customModelTrainingService = serviceProvider.GetRequiredService<ICustomModelTrainingService>();
            ISealMeetingProcessService sealMeetingService = serviceProvider.GetRequiredService<ISealMeetingProcessService>();
            ICustomVocabularyProcessService customVocabularyService = serviceProvider.GetRequiredService<ICustomVocabularyProcessService>();

            // Log the incoming payload for debugging and tracing purposes
            // This helps with troubleshooting and monitoring Lambda executions
            logger.LogInformation("Lambda Payload: {@stepfunctionPayload}", stepfunctionPayload);
            
            try
            {
                // Process different job types based on the StepFunctionJobType enum
                // Each case handles a specific type of job by delegating to the appropriate service
                switch (stepfunctionPayload.StepFunctionJobType)
                {
                    case StepFunctionJobType.Transcribe:
                        // Handle transcription requests using Amazon Transcribe
                        // Processes audio files to generate text transcriptions
                        logger.LogInformation("Processing Transcribe request for meeting: {@Id}", stepfunctionPayload.TranscribeInput?.Id);
                        if (stepfunctionPayload.TranscribeInput == null)
                        {
                            throw new Exception("Transcribe object is null");
                        }

                        var transcribeResult = await transcribeService.ProcessTranscribeRequest(stepfunctionPayload.TranscribeInput);
                        return new OkObjectResult(transcribeResult);
                        
                    case StepFunctionJobType.PromptProcess:
                        // Handle prompt processing requests for LLM interactions
                        // Processes prompts against language models to generate insights
                        logger.LogInformation("Processing Prompt request for meeting: {@MeetingId}", stepfunctionPayload.PromptProcessInput?.MeetingId);
                        if (stepfunctionPayload.PromptProcessInput == null)
                        {
                            throw new Exception("Prompt object is null");
                        }

                        var promptResult = await promptProcessService.ProcessPromptRequest(stepfunctionPayload.PromptProcessInput);
                        return new OkObjectResult(promptResult);
                        
                    case StepFunctionJobType.CustomModel:
                        // Handle custom model training requests
                        // Creates or updates custom language models for improved transcription accuracy
                        logger.LogInformation("Processing Custom Model Update request");
                        if (stepfunctionPayload.CustomModelInput == null)
                        {
                            throw new Exception("Custom model object is null");
                        }

                        var modelResult = await customModelTrainingService.ProcessCustomModelTrainingRequest(stepfunctionPayload.CustomModelInput);
                        return new OkObjectResult(modelResult);
                        
                    case StepFunctionJobType.SealMeeting:
                        // Handle meeting finalization/sealing requests
                        // Finalizes meeting data and marks it as complete/sealed
                        logger.LogInformation("Processing Seal Meeting request");
                        if (stepfunctionPayload.SealMeetingInput == null)
                        {
                            throw new Exception("Seal meeting object is null");
                        }

                        var sealMeetingResult = await sealMeetingService.SealMeetingService(stepfunctionPayload.SealMeetingInput);
                        return new OkObjectResult(sealMeetingResult);
                        
                    case StepFunctionJobType.CustomVocabulary:
                        // Handle custom vocabulary processing for transcription accuracy
                        // Creates or updates custom vocabularies to improve transcription of domain-specific terms
                        logger.LogInformation("Processing Custom Vocabulary request");
                        if (stepfunctionPayload.CustomVocabularyInput == null)
                        {
                            throw new Exception("Custom vocabulary object is null");
                        }

                        var vocabularyResult = await customVocabularyService.ProcessCustomVocabularyRequest(stepfunctionPayload.CustomVocabularyInput);
                        return new OkObjectResult(vocabularyResult);
                        
                    default:
                        // Handle unknown job types
                        // Logs an error and throws an exception for unrecognized job types
                        logger.LogError("StepFunctionJobType not found: {@stepfunctionPayload}", stepfunctionPayload);
                        throw new Exception("StepFunctionJobType not found");
                }
            }
            catch (Exception ex)
            {
                // Log and return any errors that occur during processing
                // This ensures errors are properly captured in CloudWatch logs
                logger.LogError("Error processing request: {@ex}", ex);
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
