// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StepFunctionLambda.Services;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace StepFunctionLambda
{
    public class LambdaEntryPoint
    {
        public async Task<IActionResult> FunctionHandlerAsync(StepFunctionCombinedInputType stepfunctionPayload)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            var serviceCollection = new ServiceCollection();
            var startup = new Startup(configuration);
            startup.ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            ILogger logger = serviceProvider.GetRequiredService<ILogger<LambdaEntryPoint>>();
            ITranscribeService transcribeService = serviceProvider.GetRequiredService<ITranscribeService>();
            IPromptProcessService promptProcessService = serviceProvider.GetRequiredService<IPromptProcessService>();
            ICustomModelTrainingService customModelTrainingService = serviceProvider.GetRequiredService<ICustomModelTrainingService>();
            ISealMeetingProcessService sealMeetingService = serviceProvider.GetRequiredService<ISealMeetingProcessService>();
            ICustomVocabularyProcessService customVocabularyService = serviceProvider.GetRequiredService<ICustomVocabularyProcessService>();

            logger.LogInformation("Lambda Payload: {@stepfunctionPayload}", stepfunctionPayload);
            try
            {
                switch (stepfunctionPayload.StepFunctionJobType)
                {
                    case StepFunctionJobType.Transcribe:
                        logger.LogInformation("Processing Transcribe request for meeting: {@Id}", stepfunctionPayload.TranscribeInput?.Id);
                        if (stepfunctionPayload.TranscribeInput == null)
                        {
                            throw new Exception("Transcribe object is null");
                        }

                        var transcribeResult = await transcribeService.ProcessTranscribeRequest(stepfunctionPayload.TranscribeInput);
                        return new OkObjectResult(transcribeResult);
                    case StepFunctionJobType.PromptProcess:
                        logger.LogInformation("Processing Prompt request for meeting: {@MeetingId}", stepfunctionPayload.PromptProcessInput?.MeetingId);
                        if (stepfunctionPayload.PromptProcessInput == null)
                        {
                            throw new Exception("Prompt object is null");
                        }

                        var promptResult = await promptProcessService.ProcessPromptRequest(stepfunctionPayload.PromptProcessInput);
                        return new OkObjectResult(promptResult);
                    case StepFunctionJobType.CustomModel:
                        logger.LogInformation("Processing Custom Model Update request");
                        if (stepfunctionPayload.CustomModelInput == null)
                        {
                            throw new Exception("Custom model object is null");
                        }

                        var modelResult = await customModelTrainingService.ProcessCustomModelTrainingRequest(stepfunctionPayload.CustomModelInput);
                        return new OkObjectResult(modelResult);
                    case StepFunctionJobType.SealMeeting:
                        logger.LogInformation("Processing Seal Meeting request");
                        if (stepfunctionPayload.SealMeetingInput == null)
                        {
                            throw new Exception("Seal meeting object is null");
                        }

                        var sealMeetingResult = await sealMeetingService.SealMeetingService(stepfunctionPayload.SealMeetingInput);
                        return new OkObjectResult(sealMeetingResult);
                    case StepFunctionJobType.CustomVocabulary:
                        logger.LogInformation("Processing Custom Vocabulary request");
                        if (stepfunctionPayload.CustomVocabularyInput == null)
                        {
                            throw new Exception("Custom vocabulary object is null");
                        }

                        var vocabularyResult = await customVocabularyService.ProcessCustomVocabularyRequest(stepfunctionPayload.CustomVocabularyInput);
                        return new OkObjectResult(vocabularyResult);
                    default:
                        logger.LogError("StepFunctionJobType not found: {@stepfunctionPayload}", stepfunctionPayload);
                        throw new Exception("StepFunctionJobType not found");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error processing request: {@ex}", ex);
                return new BadRequestObjectResult(ex);
            }
        }
    }
}