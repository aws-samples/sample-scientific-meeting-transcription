// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common.Types.StepFunction;

namespace StepFunctionLambda.Services;

/// <summary>
/// Interface for the Prompt Processing service that handles LLM prompt requests.
/// Provides functionality to process prompts against language models to generate insights from meeting transcripts.
/// </summary>
public interface IPromptProcessService
{
    /// <summary>
    /// Processes a prompt request against a language model.
    /// Takes meeting transcript data and prompt templates, submits them to an LLM,
    /// and processes the responses to extract insights.
    /// </summary>
    /// <param name="input">Input parameters containing meeting ID, prompt details, and processing settings</param>
    /// <returns>Object containing the result of the prompt processing operation</returns>
    Task<object> ProcessPromptRequest(PromptProcessLambdaInputType input);
}
