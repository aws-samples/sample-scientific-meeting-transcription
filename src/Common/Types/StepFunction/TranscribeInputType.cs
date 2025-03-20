// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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