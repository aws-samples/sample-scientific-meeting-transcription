// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;

namespace Common.Types.StepFunction
{
    public class PromptProcessLambdaInputType
    {
        [JsonPropertyName("meeting_id")] public Guid? MeetingId { get; set; }
    }
}