// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common.Types.StepFunction;

namespace StepFunctionLambda.Services;

/// <summary>
/// Interface for the Custom Model Training service that handles creation and training of custom language models.
/// Provides functionality to create or update custom models for improved transcription accuracy.
/// </summary>
public interface ICustomModelTrainingService
{
    /// <summary>
    /// Processes a custom model training request.
    /// Creates or updates a custom language model using provided training data,
    /// monitors the training job, and updates model status in the database.
    /// </summary>
    /// <param name="input">Input parameters containing model details, training data location, and training settings</param>
    /// <returns>Object containing the result of the custom model training operation</returns>
    Task<object> ProcessCustomModelTrainingRequest(CustomModelLambdaInputType input);
}
