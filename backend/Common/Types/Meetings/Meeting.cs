// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Types.CustomModels;
using Common.Types.CustomVocabularies;
using Common.Types.MeetingDocuments;
using Common.Types.MeetingPromptResponses;
using Common.Types.PromptSets;
using Common.Types.Teams;

namespace Common.Types.Meetings
{
    public class MeetingDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Guid? PromptSetId { get; set; }
        public Guid? CustomModelId { get; set; }

        public Guid? CustomVocabularyId { get; set; }
        public CustomVocabularyDatabaseType? CustomVocabulary { get; set; }
        public CustomModelDatabaseType? CustomModel { get; set; }
        public PromptSetDatabaseType? PromptSet { get; set; }
        public TeamDatabaseType? Team { get; set; }
        public List<MeetingPromptResponseDatabaseType>? MeetingPromptResponses { get; set; }
        public Guid? TeamId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public CurrentStepEnum? CurrentStep { get; set; }
        public StatusEnum? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? PreSignedUrl { get; set; }
        public string? S3RecordingFullPath { get; set; }
        public string? S3TranscribedFullPath { get; set; }
        public string? TranscribedPreSignedUrl { get; set; }
        public string? StateMachineExecutionArn { get; set; }
        public string? S3OutputBucketName { get; set; }
        public string? S3OutputKeyName { get; set; }
        public string? TranscribeError { get; set; }
        public string? MeetingNotes { get; set; }
        public int? MeetingNotesVersion { get; set; }
        public bool? IncludeInModelTraining { get; set; }
        public JsonDocument? MeetingAnalyticsPayload { get; set; }
        public List<MeetingDocumentDatabaseType>? MeetingDocuments { get; set; }
        public virtual string MeetingNotesVersionLocation => $"teams/{TeamId}/meetings/{Id}/meeting_notes_{MeetingNotesVersion}.json";
        public virtual string MeetingNotesVersionLocationUpdate => $"teams/{TeamId}/meetings/{Id}/meeting_notes_{MeetingNotesVersion + 1}.json";
        public virtual string MeetingRecordingLocation => $"teams/{TeamId}/meetings/{Id}/recording/{Id}.mp4";
        
        public virtual List<CanPerformEnum> CanPerformValue
        {
            get
            {
                var canPerform = new List<CanPerformEnum>();
                switch (CurrentStep)
                {
                    case CurrentStepEnum.Created:
                        canPerform.Add(CanPerformEnum.Upload);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.Edit);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.Uploaded:
                        canPerform.Add(CanPerformEnum.Transcribe);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.Edit);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.Completed:
                        canPerform.Add(CanPerformEnum.Seal);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.Edit);
                        canPerform.Add(CanPerformEnum.ReviewNotes);
                        canPerform.Add(CanPerformEnum.ViewPromptNotes);
                        canPerform.Add(CanPerformEnum.GenerateNotes);
                        canPerform.Add(CanPerformEnum.ExportNotes);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.NotesReviewed:
                        canPerform.Add(CanPerformEnum.GenerateNotes);
                        canPerform.Add(CanPerformEnum.Seal);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.Edit);
                        canPerform.Add(CanPerformEnum.ExportNotes);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.PromptProcessed:
                    case CurrentStepEnum.Transcribed:
                        canPerform.Add(CanPerformEnum.GenerateNotes);
                        canPerform.Add(CanPerformEnum.Seal);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.Transcribe);
                        canPerform.Add(CanPerformEnum.Edit);
                        canPerform.Add(CanPerformEnum.ViewPromptNotes);
                        canPerform.Add(CanPerformEnum.ReviewNotes);
                        canPerform.Add(CanPerformEnum.ExportNotes);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.Transcribing:
                    case CurrentStepEnum.PromptProcessing:
                    case CurrentStepEnum.Sealing:
                        canPerform.Add(CanPerformEnum.Nothing);
                        break;
                    case CurrentStepEnum.TranscribeFailed:
                    case CurrentStepEnum.Sealed:
                        canPerform.Add(CanPerformEnum.ViewAnalytics);
                        canPerform.Add(CanPerformEnum.Transcribe);
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.ExportNotes);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    case CurrentStepEnum.SealFailed:
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.ExportNotes);
                        canPerform.Add(CanPerformEnum.Seal);
                        canPerform.Add(CanPerformEnum.ViewDocuments);

                        break;
                    case CurrentStepEnum.PromptsFailed:
                        canPerform.Add(CanPerformEnum.Delete);
                        canPerform.Add(CanPerformEnum.GenerateNotes);
                        canPerform.Add(CanPerformEnum.ViewDocuments);
                        break;
                    default:
                        canPerform.Add(CanPerformEnum.Delete);
                        break;
                }

                return canPerform;
            }
        }
    }


    public enum CurrentStepEnum
    {
        Created,
        Uploaded,
        Transcribing,
        Transcribed,
        TranscribeFailed,
        NotesReviewed,
        Completed,
        PromptProcessing,
        PromptsFailed,
        PromptProcessed,
        Sealed,
        Sealing,
        SealFailed
    }

    public enum CanPerformEnum
    {
        Upload,
        Nothing,
        Transcribe,
        GenerateNotes,
        Seal,
        Edit,
        Delete,
        ReviewNotes,
        ViewPromptNotes,
        ExportNotes,
        ViewAnalytics,
        ViewDocuments
    }

    public class MeetingRequestType
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("date")] public DateTime? Date { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("current_step")]
        public CurrentStepEnum? CurrentStep { get; set; }

        [JsonPropertyName("prompt_set_id")] public Guid? PromptSetId { get; set; }
        [JsonPropertyName("custom_model_id")] public Guid? CustomModelId { get; set; }

        [JsonPropertyName("custom_vocabulary_id")]
        public Guid? CustomVocabularyId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public StatusEnum? Status { get; set; }

        [JsonPropertyName("process_transcription_llm")]
        public bool? ProcessTranscriptionLLM { set; get; }

        [JsonPropertyName("meeting_notes")] public string? MeetingNotes { get; set; }

        [JsonPropertyName("include_in_model_training")]
        public bool? IncludeInModelTraining { get; set; }

        [JsonPropertyName("meeting_notes_version")]
        public int? MeetingNotesVersion { get; set; }
    }

    public class MeetingResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("prompt_set_id")] public Guid? PromptSetId { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }

        [JsonPropertyName("custom_model_id")] public Guid? CustomModelId { get; set; }
        [JsonPropertyName("custom_model")] public CustomModelResponseType? CustomModel { get; set; }

        [JsonPropertyName("custom_vocabulary_id")]
        public Guid? CustomVocabularyId { get; set; }

        [JsonPropertyName("custom_vocabulary")]
        public CustomVocabularyResponseType? CustomVocabulary { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("current_step")]
        public CurrentStepEnum? CurrentStep { get; set; }

        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("team")] public TeamResponseType? Team { get; set; }
        [JsonPropertyName("promptset")] public PromptSetResponseType? PromptSet { get; set; }
        [JsonPropertyName("pre_signed_url")] public string? PreSignedUrl { get; set; }
        [JsonPropertyName("date")] public DateTime? Date { get; set; }

        [JsonPropertyName("meeting_documents")]
        public List<MeetingDocumentDatabaseType>? MeetingDocuments { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public StatusEnum? Status { get; set; }

        [JsonPropertyName("transcribed_presigned_url")]
        public string? TranscribedPreSignedUrl { get; set; }

        [JsonPropertyName("s3_transcribed_full_path")]
        public string? S3TranscribedFullPath { get; set; }

        [JsonPropertyName("s3_recording_full_path")]
        public string? S3RecordingFullPath { get; set; }

        [JsonPropertyName("s3_output_bucket_name")]
        public string? S3OutputBucketName { get; set; }

        [JsonPropertyName("s3_output_key_name")]
        public string? S3OutputKeyName { get; set; }

        [JsonPropertyName("transcribe_error")] public string? TranscribeError { get; set; }

        [JsonPropertyName("include_in_model_training")]
        public bool? IncludeInModelTraining { get; set; }

        [JsonPropertyName("can_perform")] public List<CanPerformEnum>? CanPerformValue { get; set; }

        [JsonPropertyName("meeting_notes_version")]
        public int? MeetingNotesVersion { get; set; }
    }
}