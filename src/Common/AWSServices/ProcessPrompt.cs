// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.Logging;

namespace Common.AWSServices;

/// <summary>
/// Static class for processing prompts using Amazon Bedrock
/// Provides functionality to analyze meeting content using Claude model
/// </summary>
public static class BedrockProcessor
{
    /// <summary>
    /// Processes a prompt request using Bedrock Claude model
    /// </summary>
    /// <param name="_userPrompt">The user's question or prompt</param>
    /// <param name="meetingNotes">The meeting transcript or notes to analyze</param>
    /// <param name="logger">Logger for tracking operations</param>
    /// <param name="bedrockRuntimeClient">The Bedrock runtime client</param>
    /// <param name="_systemPrompt">Optional custom system prompt (defaults to meeting analysis prompt)</param>
    /// <returns>The processed response from Claude</returns>
    public static async Task<string?> ProcessRequest(string _userPrompt, string? meetingNotes, ILogger logger, AmazonBedrockRuntimeClient bedrockRuntimeClient,
        string? _systemPrompt = null)
    {
        string systemPrompt;
        string userPrompt;
        
        if (_systemPrompt == null)
        {
            // Default system prompt for meeting analysis
            systemPrompt =
                "You are an expert meeting analyst specializing in extracting key information from meeting transcripts and notes. Your task is to analyze the following meeting content and provide a clear answer to the question being asked. \n\r " +
                "Output multiple items with bullet points each on their own line. \n\r" +
                "Present your analysis in a clean, professional format without repeating the original question. Focus solely on extracting and organizing the most relevant information.";
                
            // Combine user question with meeting notes
            userPrompt = $"""
                          Question: {_userPrompt} \n\r
                          MeetingNotes: \n\r
                          {meetingNotes}
                          """;
        }
        else
        {
            // Use provided system prompt and user prompt directly
            systemPrompt = _systemPrompt;
            userPrompt = _userPrompt;
        }
        
        // Create Claude client and process the request
        BedrockClaudeClient claudeClient = new BedrockClaudeClient(logger);
        return await claudeClient.Invoke(userPrompt, systemPrompt, bedrockRuntimeClient);
    }
}