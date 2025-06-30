// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Bedrock;
using Amazon.Bedrock.Model;

namespace Common.AWSServices;

/// <summary>
/// Class for managing Bedrock inference profiles
/// Provides methods to retrieve and filter available inference profiles
/// </summary>
public class InferenceProfiles
{
    /// <summary>
    /// Retrieves a specific inference profile ARN by model name
    /// </summary>
    /// <param name="bedrockService">The Bedrock service client</param>
    /// <param name="model">The model name to search for (e.g., "Claude", "Nova")</param>
    /// <returns>The matching inference profile summary or null if not found</returns>
    public async Task<InferenceProfileSummary?> GetInferenceProfileArnAsync(IAmazonBedrock bedrockService, string? model)
    {
        // Get all available inference profiles
        InferenceProfileSummary? inferenceProfileSummary = null;
        List<InferenceProfileSummary>? inferenceProfileSummaries = await GetAllInferenceProfilesAsync(bedrockService);
        
        // Find the first profile that contains the specified model name
        if (inferenceProfileSummaries != null)
        {
            inferenceProfileSummary = inferenceProfileSummaries.FirstOrDefault(x => x.InferenceProfileName?.Contains(model) ?? false);
        }

        return inferenceProfileSummary;
    }

    /// <summary>
    /// Retrieves all available inference profiles with optional filtering
    /// </summary>
    /// <param name="bedrockService">The Bedrock service client</param>
    /// <param name="modelFilter">Optional list of model name filters (defaults to Claude and Nova)</param>
    /// <returns>List of matching inference profile summaries</returns>
    public async Task<List<InferenceProfileSummary>?> GetAllInferenceProfilesAsync(IAmazonBedrock bedrockService, List<string>? modelFilter = null)
    {
        // Default to Claude and Nova models if no filter is provided
        if (modelFilter == null)
        {
            modelFilter = ["Claude", "Nova"];
        }

        // Request up to 100 inference profiles from Bedrock
        var availableProfiles = await bedrockService.ListInferenceProfilesAsync(new ListInferenceProfilesRequest()
        {
            MaxResults = 100
        });
        
        // Filter profiles by the specified model names
        var models = availableProfiles?
            .InferenceProfileSummaries?
            .FindAll(x => modelFilter.Any(filter => x?.InferenceProfileName?.Contains(filter) == true));
            
        return models;
    }
}