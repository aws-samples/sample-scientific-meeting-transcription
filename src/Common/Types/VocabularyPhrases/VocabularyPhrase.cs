// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;
using Common.Types.CustomVocabularies;
using Common.Types.Teams;

namespace Common.Types.VocabularyPhrases
{
    public class VocabularyPhraseRequestType
    {
        [JsonPropertyName("phrase")] public string? Phrase { get; set; }
        [JsonPropertyName("display_as")] public string? DisplayAs { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }
    }

    public class VocabularyPhraseDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public Guid CustomVocabularyId { get; set; }
        public Guid? TeamId { get; set; }
        public TeamDatabaseType? Team { get; set; }
        public CustomVocabularyDatabaseType? CustomVocabulary { get; set; }
        public string? Phrase { get; set; }
        public string? DisplayAs { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class VocabularyPhraseResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("phrase")] public string? Phrase { get; set; }
        [JsonPropertyName("display_as")] public string? DisplayAs { get; set; }

        [JsonPropertyName("custom_vocabulary_id")]
        public Guid CustomVocabularyId { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEnum Status { get; set; }

        [JsonPropertyName("language_code")] public string? LanguageCode { get; set; }
    }
}