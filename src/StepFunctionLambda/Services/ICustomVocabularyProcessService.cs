// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common.Types.StepFunction;

namespace StepFunctionLambda.Services;

/// <summary>
/// Interface for the Custom Vocabulary Process service that handles custom vocabulary creation and management.
/// Provides functionality to create or update custom vocabularies for improved transcription accuracy.
/// </summary>
public interface ICustomVocabularyProcessService
{
    /// <summary>
    /// Processes a custom vocabulary request.
    /// Creates or updates a custom vocabulary with domain-specific terms,
    /// submits it to Amazon Transcribe, and monitors the creation process.
    /// </summary>
    /// <param name="input">Input parameters containing vocabulary details, phrases, and processing settings</param>
    /// <returns>Object containing the result of the custom vocabulary processing operation</returns>
    Task<object> ProcessCustomVocabularyRequest(CustomVocabularyLambdaInputType input);
}
