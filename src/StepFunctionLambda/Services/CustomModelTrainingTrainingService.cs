// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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