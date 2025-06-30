// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;

namespace Common.AWSServices;

/// <summary>
/// Client for interacting with Amazon Bedrock Claude model
/// Handles API requests and response parsing for Claude LLM
/// </summary>
public class BedrockClaudeClient
{
    // Logger for tracking operations and errors
    readonly ILogger _logger;
    
    /// <summary>
    /// Constructor that initializes the client with a logger
    /// </summary>
    /// <param name="logger">Logger for tracking operations</param>
    public BedrockClaudeClient(ILogger logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Invokes the Claude model with user and system prompts
    /// </summary>
    /// <param name="userPrompt">The prompt from the user to send to Claude</param>
    /// <param name="systemPrompt">The system instructions for Claude</param>
    /// <param name="bedrockProcessor">The Bedrock runtime client</param>
    /// <returns>The text response from Claude or null if unsuccessful</returns>
    public async Task<string?> Invoke(string userPrompt, string systemPrompt, AmazonBedrockRuntimeClient bedrockProcessor)
    {
        // Log the invocation with the model ARN
        _logger.LogInformation("Invoking Claude: {InferenceProfileArn}", EnvironmentHelper.BEDROCK_MODEL_INFERENCE_ARN);
        
        // Prepare the request with model configuration and prompts
        var request = new InvokeModelRequest
        {
            ModelId = EnvironmentHelper.BEDROCK_MODEL_INFERENCE_ARN,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(new
                    {
                        anthropic_version = "bedrock-2023-05-31", // API version for Claude
                        max_tokens = 4096, // Maximum tokens in the response
                        system = systemPrompt, // System instructions
                        messages = new[]
                        {
                            new { role = "user", content = userPrompt } // User message
                        }
                    })
                )
            )
        };

        string? responseText = String.Empty;
        try
        {
            // Send the request to Bedrock
            var response = await bedrockProcessor.InvokeModelAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                // Parse the JSON response
                var modelResponse = await JsonNode.ParseAsync(response.Body);
                // Extract the text content from the response
                responseText = modelResponse?["content"]?[0]?["text"]?.ToString();
                
                // Log successful invocation with token usage metrics
                _logger.LogInformation("Successfully invoked Claude: intokens: {input_tokens} | outtokens: {output_tokens}", 
                    modelResponse?["usage"]?["input_tokens"], 
                    modelResponse?["usage"]?["output_tokens"]);
            }
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            // Log and rethrow any Bedrock-specific errors
            _logger.LogError("Error invoked Claude: {@ex}", ex);
            throw new Exception($"Claude processing error: {ex.Message}");
        }

        return responseText;
    }
}