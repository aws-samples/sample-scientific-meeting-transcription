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

public class BedrockNovaClient
{
    public async Task<string> Invoke(string modelId, string userPrompt, string systemPrompt, AmazonBedrockRuntimeClient bedrockClient)
    {
        var requestObject = JsonSerializer.Serialize(BedrockNovaType.CreateUserRequest(userPrompt, systemPrompt));
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
            var response = await bedrockClient.InvokeModelAsync(request);
            if (response == null) throw new ArgumentNullException(nameof(response));
            using var streamReader = new StreamReader(response.Body);
            var jsonResponse = await streamReader.ReadToEndAsync();
            var modelResponse = JsonSerializer.Deserialize<JsonNode>(jsonResponse);
            responseText = modelResponse?["output"]?["message"]?["content"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception($"Unable to parse response: {modelResponse}");
            }
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            throw new Exception($"Nova processing error: {ex.Message}");
        }

        return responseText;
    }
}