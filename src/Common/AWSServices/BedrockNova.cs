// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Common.Types.Bedrock;

namespace Common.AWSServices;

/// <summary>
/// Client for interacting with Amazon Bedrock Nova model
/// Handles API requests and response parsing for Nova LLM
/// </summary>
public class BedrockNovaClient
{
    /// <summary>
    /// Invokes the Nova model with specified prompts and configuration
    /// </summary>
    /// <param name="modelId">The ID of the Nova model to use</param>
    /// <param name="userPrompt">The prompt from the user to send to Nova</param>
    /// <param name="systemPrompt">The system instructions for Nova</param>
    /// <param name="bedrockClient">The Bedrock runtime client</param>
    /// <returns>The text response from Nova</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null</exception>
    /// <exception cref="Exception">Thrown when response parsing fails or Nova processing errors occur</exception>
    public async Task<string> Invoke(string modelId, string userPrompt, string systemPrompt, AmazonBedrockRuntimeClient bedrockClient)
    {
        // Create and serialize the request object with user and system prompts
        var requestObject = JsonSerializer.Serialize(BedrockNovaType.CreateUserRequest(userPrompt, systemPrompt));
        
        // Configure the model invocation request
        var request = new InvokeModelRequest()
        {
            ModelId = modelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(requestObject))
        };

        string? responseText;
        try
        {
            // Send request to Bedrock and process response
            var response = await bedrockClient.InvokeModelAsync(request);
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            // Read and parse the JSON response
            using var streamReader = new StreamReader(response.Body);
            var jsonResponse = await streamReader.ReadToEndAsync();
            var modelResponse = JsonSerializer.Deserialize<JsonNode>(jsonResponse);
            
            // Extract the text content from the response
            responseText = modelResponse?["output"]?["message"]?["content"]?[0]?["text"]?.ToString();

            // Validate response content
            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception($"Unable to parse response: {modelResponse}");
            }
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            // Handle Bedrock-specific errors
            throw new Exception($"Nova processing error: {ex.Message}");
        }

        return responseText;
    }
}