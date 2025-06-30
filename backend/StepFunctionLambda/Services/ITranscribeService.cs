// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Common.Types.StepFunction;

namespace StepFunctionLambda.Services;

/// <summary>
/// Interface for the Transcribe service that handles audio transcription requests.
/// Provides functionality to process transcription jobs using Amazon Transcribe.
/// </summary>
public interface ITranscribeService
{
    /// <summary>
    /// Processes a transcription request for a meeting audio file.
    /// Submits the audio file to Amazon Transcribe, monitors job progress,
    /// and stores the resulting transcription data.
    /// </summary>
    /// <param name="input">Input parameters containing meeting ID, S3 location of audio file, and other transcription settings</param>
    /// <returns>Object containing the result of the transcription process</returns>
    Task<object> ProcessTranscribeRequest(TranscriptionMeetingInputType input);
}
