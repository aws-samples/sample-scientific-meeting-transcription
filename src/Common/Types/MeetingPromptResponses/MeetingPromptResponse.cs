// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.Teams;

namespace Common.Types.MeetingPromptResponses
{
    public class MeetingPromptResponseDatabaseType : IEntityDate
    {
        public Guid Id { get; set; }
        public string? Prompt { get; set; }
        public string? PromptResponse { get; set; }
        public Guid? TeamId { get; set; }
        public virtual TeamDatabaseType? Team { get; set; }
        public Guid? MeetingId { get; set; }
        public virtual MeetingDatabaseType? Meeting { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class MeetingPromptResponseRequestType
    {
        [JsonPropertyName("prompt_response")] public string? PromptResponse { get; set; }
        [JsonPropertyName("prompt_id")] public Guid? PromptId { get; set; }
    }

    public class MeetingPromptResponseResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("prompt")] public string? Prompt { get; set; }
        [JsonPropertyName("prompt_response")] public string? PromptResponse { get; set; }
    }

    public class MeetingPromptResponseSlimType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid? Id { get; set; }
        [JsonPropertyName("prompt")] public string? Prompt { get; set; }
        [JsonPropertyName("prompt_response")] public string? PromptResponse { get; set; }
    }
}