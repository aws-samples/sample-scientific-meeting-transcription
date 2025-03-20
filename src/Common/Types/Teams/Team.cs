// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Types.CustomModels;
using Common.Types.CustomVocabularies;
using Common.Types.MeetingPromptResponses;
using Common.Types.Meetings;
using Common.Types.PromptSets;

namespace Common.Types.Teams
{
    public sealed class TeamDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public string? Team { get; set; }
        public List<MeetingDatabaseType>? Meetings { get; set; }
        public List<CustomModelDatabaseType>? CustomModels { get; set; }
        public List<PromptSetDatabaseType>? PromptSets { get; set; }
        public List<MeetingPromptResponseDatabaseType>? MeetingPromptResponses { get; set; }
        public List<CustomVocabularyDatabaseType>? CustomVocabularies { get; set; }
        public string? IdpGroup { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TeamRequestType
    {
        [JsonPropertyName("team")] public string? Team { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public StatusEnum? Status { get; set; }

        [JsonPropertyName("idp_group")] public string? IdpGroup { get; set; }
    }

    public class TeamResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid? Id { get; set; }
        [JsonPropertyName("team")] public string? Team { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public StatusEnum? Status { get; set; }

        [JsonPropertyName("idp_group")] public string? IdpGroup { get; set; }
    }
}