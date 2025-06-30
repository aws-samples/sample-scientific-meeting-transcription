// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common;
using Common.Types.StepFunction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace StepFunctionLambda.Services;

public class CustomModelTrainingTrainingService(ILogger<CustomModelTrainingTrainingService> logger, ApplicationDbContext dbContext) : ICustomModelTrainingService
{
    public async Task<object> ProcessCustomModelTrainingRequest(CustomModelStepMachineType input)
    {
        logger.LogInformation("Data received: {@input}", input);
        try
        {
            var customModelDatabaseRecord = await dbContext.CustomModels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            customModelDatabaseRecord!.ModelSetupProgress = input.ModelSetupProgress;
            customModelDatabaseRecord.ModelSetupMessage = input.SetupError?.Cause?.Substring(0, 500);
            dbContext.Entry(customModelDatabaseRecord).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return new OkResult();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing custom model request");
            throw;
        }
    }
}