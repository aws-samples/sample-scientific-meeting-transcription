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

public class BedrockClaudeClient
{
    readonly ILogger _logger;
    public BedrockClaudeClient(ILogger logger)
    {
        _logger = logger;
    }
    public async Task<string?> Invoke(string userPrompt, string systemPrompt, AmazonBedrockRuntimeClient bedrockProcessor)
    {
        _logger.LogInformation("Invoking Claude: {InferenceProfileArn}", EnvironmentHelper.BEDROCK_MODEL_INFERENCE_ARN);
        var request = new InvokeModelRequest
        {
            ModelId = EnvironmentHelper.BEDROCK_MODEL_INFERENCE_ARN,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(new
                    {
                        anthropic_version = "bedrock-2023-05-31",
                        max_tokens = 4096,
                        system = systemPrompt,
                        messages = new[]
                        {
                            new { role = "user", content = userPrompt }
                        }
                    })
                )
            )
        };

        string? responseText = String.Empty;
        try
        {
            var response = await bedrockProcessor.InvokeModelAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var modelResponse = await JsonNode.ParseAsync(response.Body);
                responseText = modelResponse?["content"]?[0]?["text"]?.ToString();
                
                _logger.LogInformation("Successfully invoked Claude: intokens: {input_tokens} | outtokens: {output_tokens}", modelResponse?["usage"]?["input_tokens"], modelResponse?["usage"]?["output_tokens"]);

            }
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            _logger.LogError("Error invoked Claude: {@ex}", ex);

            throw new Exception($"Claude processing error: {ex.Message}");
        }

        return responseText;
    }
}