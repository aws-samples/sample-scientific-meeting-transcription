// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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