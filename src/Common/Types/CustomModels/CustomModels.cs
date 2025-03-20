// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Types.Meetings;
using Common.Types.Teams;

namespace Common.Types.CustomModels
{
    public class CustomModelRequestType
    {
        [JsonPropertyName("model_name")] public string? ModelName { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("transcribe_base_model_name")]
        public TranscribeBaseModel? TranscribeBaseModel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("custom_model_progress_status")]
        public CustomModelSetupProgressEnum ModelSetupProgress { get; set; }
    }

    public class CustomModelDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public Guid? TeamId { get; set; }
        public TeamDatabaseType? Team { get; set; }
        public string? ModelName { get; set; }
        public string? Description { get; set; }
        public string? LanguageCode { get; set; }
        public string? DataAccessRoleArn { get; set; }
        public string? TranscribedDataS3Uri { get; set; }
        public string? TrainingDataS3Uri { get; set; }
        public string? TrainingDataS3UriFolder { get; set; }
        public string? AwsModelStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public StatusEnum Status { get; set; }
        public CustomModelSetupProgressEnum? ModelSetupProgress { get; set; }
        public string? ModelSetupMessage { get; set; }
        public string? PreSignedUrl { get; set; }
        public DateTime? PreSignedUrlExpire { get; set; }
        public string? StateMachineExecutionArn { get; set; }
        public TranscribeBaseModel? TranscribeBaseModel { get; set; }
        public List<MeetingDatabaseType>? Meetings { get; set; }

        public virtual List<CustomModelCanPerformEnum> CanPerformValue
        {
            get
            {
                var canPerform = new List<CustomModelCanPerformEnum>();
                switch (ModelSetupProgress)
                {
                    case CustomModelSetupProgressEnum.Created:
                    case CustomModelSetupProgressEnum.S3SignedUrlCreated:
                        canPerform.Add(CustomModelCanPerformEnum.Delete);
                        canPerform.Add(CustomModelCanPerformEnum.Edit);
                        canPerform.Add(CustomModelCanPerformEnum.Upload);
                        break;
                    case CustomModelSetupProgressEnum.TrainingDataUploaded:
                    case CustomModelSetupProgressEnum.ModelReady:
                        canPerform.Add(CustomModelCanPerformEnum.Delete);
                        canPerform.Add(CustomModelCanPerformEnum.Edit);
                        canPerform.Add(CustomModelCanPerformEnum.Upload);
                        canPerform.Add(CustomModelCanPerformEnum.Train);
                        break;
                    case CustomModelSetupProgressEnum.TrainingQueued:
                    case CustomModelSetupProgressEnum.TrainingStarted:
                        canPerform.Add(CustomModelCanPerformEnum.Nothing);
                        break;
                    case CustomModelSetupProgressEnum.TrainingFailed:
                        canPerform.Add(CustomModelCanPerformEnum.Delete);
                        break;
                    default:
                        canPerform.Add(CustomModelCanPerformEnum.Delete);
                        break;
                }

                return canPerform;
            }
        }
    }

    public enum TranscribeBaseModel
    {
        NarrowBand,
        WideBand
    }

    public enum CustomModelSetupProgressEnum
    {
        Created,
        S3SignedUrlCreated,
        TrainingDataUploaded,
        TrainingQueued,
        TrainingStarted,
        ModelReady,
        TrainingFailed
    }

    public enum CustomModelCanPerformEnum
    {
        Edit,
        Delete,
        Train,
        Upload,
        Nothing
    }

    public class CustomModelResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("model_name")] public string? ModelName { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public StatusEnum Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("custom_model_progress_status")]
        public CustomModelSetupProgressEnum ModelSetupProgress { get; set; }

        [JsonPropertyName("model_setup_message")]
        public string? ModelSetupMessage { get; set; }

        [JsonPropertyName("presigned_url")] public string? PreSignedUrl { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("transcribe_base_model_name")]
        public TranscribeBaseModel? TranscribeBaseModel { get; set; }

        [JsonPropertyName("presigned_url_expire")]
        public DateTime? PreSignedUrlExpire { get; set; }

        [JsonPropertyName("transcribed_data_s3_uri")]
        public string? TranscribedDataS3Uri { get; set; }

        [JsonPropertyName("training_data_s3_uri")]
        public string? TrainingDataS3Uri { get; set; }

        [JsonPropertyName("training_data_s3_uri_folder")]
        public string? TrainingDataS3UriFolder { get; set; }

        [JsonPropertyName("team")] public TeamDatabaseType? Team { get; set; }
        [JsonPropertyName("can_perform")] public List<CustomModelCanPerformEnum>? CanPerformValue { get; set; }
    }
}