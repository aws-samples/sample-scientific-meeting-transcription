// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.Teams;

namespace Common.Types.PromptSets;

public class PromptSetRequestType
{
    [JsonPropertyName("prompt_set_name")] public string? PromptSetName { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    public StatusEnum Status { get; set; }

    [JsonPropertyName("create_prompts_from_description")]
    public bool CreatePromptsFromDescription { get; set; }
}

public class PromptSetResponseType : DateTimeStamps
{
    [JsonPropertyName("id")] public Guid? Id { get; set; }
    [JsonPropertyName("prompt_set_name")] public string? PromptSetName { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("prompts")] public List<PromptResponseType>? Prompts { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    public StatusEnum Status { get; set; }

    [JsonPropertyName("create_prompts_from_description")]
    public bool CreatePromptsFromDescription { get; set; }
}

public sealed class PromptSetDatabaseType : IEntityDate
{
    public Guid Id { get; set; }
    public List<PromptDatabaseType>? Prompts { get; set; }
    public List<MeetingDatabaseType>? Meetings { get; set; }
    public TeamDatabaseType? Team { get; set; }
    public Guid? TeamId { get; set; }
    public string? PromptSetName { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public StatusEnum Status { get; set; }
    public bool? CreatePromptsFromDescription { get; set; }
}