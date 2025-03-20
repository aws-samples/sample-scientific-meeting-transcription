// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;

namespace Common.Types.Teams;

public class ChatBotRequestType
{
    [JsonPropertyName("model_id")] public String? ModelId { get; set; }
    [JsonPropertyName("meeting_id")] public Guid? MeetingId { get; set; }
    [JsonPropertyName("question")] public string? Question { get; set; }
}