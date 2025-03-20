// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Bedrock;
using Amazon.Bedrock.Model;

namespace Common.AWSServices;

public class InferenceProfiles
{
    public async Task<InferenceProfileSummary?> GetInferenceProfileArnAsync(IAmazonBedrock bedrockService, string? model)
    {
        InferenceProfileSummary? inferenceProfileSummary = null;
        List<InferenceProfileSummary>? inferenceProfileSummaries = await GetAllInferenceProfilesAsync(bedrockService);
        if (inferenceProfileSummaries != null)
        {
            inferenceProfileSummary = inferenceProfileSummaries.FirstOrDefault(x => x.InferenceProfileName?.Contains(model) ?? false);
        }

        return inferenceProfileSummary;
    }

    public async Task<List<InferenceProfileSummary>?> GetAllInferenceProfilesAsync(IAmazonBedrock bedrockService, List<string>? modelFilter = null)
    {
        if (modelFilter == null)
        {
            modelFilter = ["Claude", "Nova"];
        }

        var availableProfiles = await bedrockService.ListInferenceProfilesAsync(new ListInferenceProfilesRequest()
        {
            MaxResults = 100
        });
        var models = availableProfiles?
            .InferenceProfileSummaries?
            .FindAll(x => modelFilter.Any(filter => x?.InferenceProfileName?.Contains(filter) == true));
        return models;
    }
}