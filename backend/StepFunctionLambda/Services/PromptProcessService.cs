// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.Lambda.APIGatewayEvents;
using Common;
using Common.AWSServices;
using Common.Types.Bedrock;
using Common.Types.MeetingPromptResponses;
using Common.Types.Meetings;
using Common.Types.StepFunction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace StepFunctionLambda.Services;

public class PromptProcessService(
    ILogger<PromptProcessService> logger,
    ApplicationDbContext dbContext,
    AmazonBedrockRuntimeClient bedrockRuntimeClientService) : IPromptProcessService
{
    public async Task<object> ProcessPromptRequest(PromptProcessLambdaInputType input)
    {
        var dbMeeting = await dbContext.Meetings
            .AsNoTracking()
            .Include(x => x.PromptSet)
            .ThenInclude(x => x!.Prompts)
            .Include(x => x.MeetingPromptResponses)
            .FirstAsync(x => x.Id == input.MeetingId);

        dbContext.MeetingPromptResponses.RemoveRange(dbContext.MeetingPromptResponses.Where(x => x.MeetingId == input.MeetingId));
        await dbContext.SaveChangesAsync();

        try
        {
            //This is the general meeting summary of the meeting notes
            dbContext.MeetingPromptResponses.Add(new MeetingPromptResponseDatabaseType
            {
                TeamId = dbMeeting.TeamId,
                MeetingId = input.MeetingId,
                Prompt = "General meeting summary",
                PromptResponse =  await BedrockProcessor.ProcessRequest(
                    "Generate the summary for these meeting notes", dbMeeting.MeetingNotes!,
                    logger,
                    bedrockRuntimeClientService),
            });
            foreach (var dbPrompt in dbMeeting.PromptSet?.Prompts?.OrderBy(x => x.Order)!)
            {
                var result = await BedrockProcessor.ProcessRequest(
                    dbPrompt.Prompt!, dbMeeting.MeetingNotes!,
                    logger,
                    bedrockRuntimeClientService);
                dbContext.MeetingPromptResponses.Add(new MeetingPromptResponseDatabaseType
                {
                    TeamId = dbMeeting.TeamId,
                    MeetingId = input.MeetingId,
                    Prompt = dbPrompt.Prompt,
                    PromptResponse = result,
                });
                await dbContext.SaveChangesAsync();
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Bedrock Error: {ex}", ex);
            dbMeeting.CurrentStep = CurrentStepEnum.PromptsFailed;
            dbMeeting.TranscribeError = ex.Message;
        }

        dbMeeting.CurrentStep = CurrentStepEnum.PromptProcessed;
        dbContext.Entry(dbMeeting).State = EntityState.Modified;

        await dbContext.SaveChangesAsync();
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
}