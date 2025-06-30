// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Types.Meetings;
using Common.Types.Teams;
using Common.Types.VocabularyPhrases;

namespace Common.Types.CustomVocabularies
{
    public class CustomVocabularyRequestType
    {
        [JsonPropertyName("vocabulary_name")] public string? VocabularyName { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }
    }

    public class CustomVocabularyDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public Guid? TeamId { get; set; }
        public TeamDatabaseType? Team { get; set; }
        public string? VocabularyName { get; set; }
        public CustomVocabularyCurrentStepEnum? CurrentStep { get; set; }
        public string? PublishError { get; set; }
        public string? Description { get; set; }
        public string? LanguageCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public StatusEnum Status { get; set; }
        public List<VocabularyPhraseDatabaseType>? Phrases { get; set; }
        public List<MeetingDatabaseType>? Meetings { get; set; }

        public virtual string S3VocabularyLocation => $"teams/{TeamId}/customVocabularies/{Id}.txt";

        public virtual List<CustomVocabularyCanPerformEnum> CanPerformValue
        {
            get
            {
                var canPerform = new List<CustomVocabularyCanPerformEnum>();
                switch (CurrentStep)
                {
                    case CustomVocabularyCurrentStepEnum.PublishFailed:
                    case CustomVocabularyCurrentStepEnum.Published:
                    case CustomVocabularyCurrentStepEnum.Created:
                        canPerform.Add(CustomVocabularyCanPerformEnum.Delete);
                        canPerform.Add(CustomVocabularyCanPerformEnum.Edit);
                        canPerform.Add(CustomVocabularyCanPerformEnum.Publish);
                        canPerform.Add(CustomVocabularyCanPerformEnum.EditPhrases);
                        break;
                    case CustomVocabularyCurrentStepEnum.Publishing:
                        canPerform.Add(CustomVocabularyCanPerformEnum.Nothing);
                        break;
                    default:
                        canPerform.Add(CustomVocabularyCanPerformEnum.Delete);
                        break;
                }

                return canPerform;
            }
        }
    }

    public enum CustomVocabularyCurrentStepEnum
    {
        Created,
        Publishing,
        Published,
        PublishFailed
    }

    public enum CustomVocabularyCanPerformEnum
    {
        Edit,
        Delete,
        Publish,
        Nothing,
        EditPhrases
    }


    public class CustomVocabularyResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("vocabulary_name")] public string? VocabularyName { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }
        [JsonPropertyName("current_step")] public CustomVocabularyCurrentStepEnum? CurrentStep { get; set; }
        [JsonPropertyName("publish_error")] public string? PublishError { get; set; }
        [JsonPropertyName("can_perform")] public List<CustomVocabularyCanPerformEnum>? CanPerformValue { get; set; }
    }
}