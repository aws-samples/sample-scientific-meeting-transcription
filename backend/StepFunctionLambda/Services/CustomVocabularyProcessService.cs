// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Common;
using Common.AWSServices;
using Common.Types.CustomVocabularies;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace StepFunctionLambda.Services;

public class CustomVocabularyProcessService(
    ApplicationDbContext dbContext,
    IAmazonS3 s3Service,
    ILogger<CustomVocabularyProcessService> logger,
    IAmazonTranscribeService transcribeService,
    IAmazonSimpleSystemsManagement ssmService)
    : ICustomVocabularyProcessService
{
    public async Task<object> ProcessCustomVocabularyRequest(CustomVocabularyStepMachineType input)
    {
        logger.LogInformation("Data received: {@input}", input);

        var dbCustomVocabulary = dbContext.CustomVocabularies
            .Include(x => x.Phrases)
            .AsNoTracking()
            .FirstOrDefault(y => y.Id == input.Id);
        if (dbCustomVocabulary != null)
        {
            try
            {
                var ssmResponse = await ssmService.GetParameterAsync(new GetParameterRequest
                {
                    Name = EnvironmentHelper.S3_UPLOAD_BUCKET_PARAM_ID,
                    WithDecryption = true
                });
                var buketName = ssmResponse.Parameter.Value;
                var key = dbCustomVocabulary.S3VocabularyLocation;

                StringBuilder phrasesList = new StringBuilder();
                phrasesList.AppendLine(string.Join("\t", ["Phrase", "SoundsLike", "IPA", "DisplayAs"]));
                foreach (var phrase in dbCustomVocabulary.Phrases!)
                {
                    phrasesList.AppendLine(string.Join("\t", [phrase.Phrase, "", "", phrase.DisplayAs]));
                }

                PutObjectRequest putRequest = new PutObjectRequest
                {
                    InputStream = new MemoryStream(Encoding.UTF8.GetBytes(phrasesList.ToString())),
                    BucketName = buketName,
                    Key = key,
                    ContentType = "text/plain"
                };
                await s3Service.PutObjectAsync(putRequest);
                bool customVocabExists = false;
                try
                {
                    await transcribeService.GetVocabularyAsync(new GetVocabularyRequest
                    {
                        VocabularyName = dbCustomVocabulary.Id.ToString()
                    });
                    logger.LogInformation($"Updating current vocabulary: {dbCustomVocabulary.Id.ToString()}");
                    customVocabExists = true;
                }
                catch (Exception e)
                {
                    logger.LogWarning($"Custom Vocabulary does not exist: {dbCustomVocabulary.Id.ToString()} : {e.Message}");
                }

                if (customVocabExists)
                {
                    await transcribeService.UpdateVocabularyAsync(new UpdateVocabularyRequest
                    {
                        VocabularyName = dbCustomVocabulary.Id.ToString(),
                        LanguageCode = dbCustomVocabulary.LanguageCode,
                        VocabularyFileUri = $"s3://{buketName}/{key}"
                    });
                }
                else
                {
                    var vocabularyResponse = await transcribeService.CreateVocabularyAsync(new CreateVocabularyRequest
                    {
                        VocabularyName = dbCustomVocabulary.Id.ToString(),
                        LanguageCode = dbCustomVocabulary.LanguageCode,
                        VocabularyFileUri = $"s3://{buketName}/{key}"
                    });
                    if (vocabularyResponse.VocabularyState == VocabularyState.PENDING)
                    {
                        logger.LogInformation("Created new vocabulary: {@Id}", dbCustomVocabulary.Id.ToString());
                    }
                    else
                    {
                        logger.LogError("Create new vocabulary failed: {@Id} - {@FailureReason}", dbCustomVocabulary.Id.ToString(), vocabularyResponse.FailureReason);
                    }
                }

                while (true)
                {
                    var vocabulary = await transcribeService.GetVocabularyAsync(new GetVocabularyRequest
                    {
                        VocabularyName = dbCustomVocabulary.Id.ToString()
                    });
                    logger.LogInformation("Checking vocabulary status: {@Id}: {@Value}", dbCustomVocabulary.Id.ToString(), vocabulary.VocabularyState.Value);

                    if (vocabulary.VocabularyState.Value == "READY")
                    {
                        dbCustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.Published;
                        dbCustomVocabulary.PublishError = "";
                        await dbContext.SaveChangesAsync();
                        break;
                    }

                    if (vocabulary.VocabularyState.Value == "FAILED")
                    {
                        dbCustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.PublishFailed;
                        dbCustomVocabulary.PublishError = vocabulary.FailureReason;
                        break;
                    }

                    Thread.Sleep(5000);
                }

                dbContext.Entry(dbCustomVocabulary).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                return new OkResult();
            }
            catch (Exception e)
            {
                dbCustomVocabulary.CurrentStep = CustomVocabularyCurrentStepEnum.PublishFailed;
                dbCustomVocabulary.PublishError = e.Message;
                await dbContext.SaveChangesAsync();
                throw;
            }
        }

        return ApiActions.CreateResponse(HttpStatusCode.NotFound, new { message = "Custom Vocabulary not found" })!;
    }
}