// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.Logging;

namespace Common.AWSServices;

public static class BedrockProcessor
{
    public static async Task<string?> ProcessRequest(string _userPrompt, string? meetingNotes, ILogger logger, AmazonBedrockRuntimeClient bedrockRuntimeClient,
        string? _systemPrompt = null)
    {
        string systemPrompt;
        string userPrompt;
        if (_systemPrompt == null)
        {
            systemPrompt =
                "You are an expert meeting analyst specializing in extracting key information from meeting transcripts and notes. Your task is to analyze the following meeting content and provide a clear answer to the question being asked. \n\r " +
                "Output multiple items with bullet points each on their own line. \n\r" +
                "Present your analysis in a clean, professional format without repeating the original question. Focus solely on extracting and organizing the most relevant information.";
            userPrompt = $"""
                          Question: {_userPrompt} \n\r
                          MeetingNotes: \n\r
                          {meetingNotes}
                          """;
        }
        else
        {
            systemPrompt = _systemPrompt;
            userPrompt = _userPrompt;
        }
        BedrockClaudeClient claudeClient = new BedrockClaudeClient(logger);
        return await claudeClient.Invoke(userPrompt, systemPrompt, bedrockRuntimeClient);
    }
}