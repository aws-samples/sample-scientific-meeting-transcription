// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Text.Json.Serialization;
using Common.Types.Meetings;

namespace Common.Types.StepFunction
{
    public class TranscriptionMeetingInputType
    {
        [JsonPropertyName("id")] public Guid? Id { get; set; }

        [JsonPropertyName("current_step")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CurrentStepEnum? CurrentStep { get; set; }

        [JsonPropertyName("transcribe_error")] public StepFunctionErrorType? TranscribeError { get; set; }

        [JsonPropertyName("transcription_job")]
        public TranscriptionJobInputType? TranscriptionJob { get; set; }
    }

    public class TranscriptionJobInputType
    {
        [JsonPropertyName("TranscriptionJobName")]
        public string? TranscriptionJobName { get; set; }

        [JsonPropertyName("LanguageCode")] public string? LanguageCode { get; set; }

        [JsonPropertyName("MediaFileUri")] public string? MediaFileUri { get; set; }

        [JsonPropertyName("LanguageModelName")]
        public string? LanguageModelName { get; set; }

        [JsonPropertyName("OutputBucketName")] public string? OutputBucketName { get; set; }

        [JsonPropertyName("OutputKey")] public string? OutputKey { get; set; }

        [JsonPropertyName("VocabularyName")] public string? VocabularyName { get; set; }
    }
}