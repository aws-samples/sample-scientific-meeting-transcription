// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Types.StepFunction;

public class MeetingNotesType
{
    [JsonPropertyName("jobName")] public string? JobName { get; set; }
    [JsonPropertyName("transcript")] public string? Transcript { get; set; }
    [JsonPropertyName("version")] public int? Version { get; set; }
    [JsonPropertyName("last_edited_at")] public string? LastEditedAt { get; set; }
    [JsonPropertyName("last_edited_by")] public string? LastEditedBy { get; set; }
    [JsonPropertyName("segments")] public List<SpeakerSegmentType>? Segments { get; set; }
    [JsonPropertyName("words")] public List<SpeakerWordType>? Words { get; set; }
}

public class SpeakerSegmentType
{
    [JsonPropertyName("segment_id")] public int? SegmentId { get; set; }
    [JsonPropertyName("transcript")] public string? Transcript { get; set; }
    [JsonPropertyName("start_time")] public double? StartTime { get; set; }
    [JsonPropertyName("end_time")] public double? EndTime { get; set; }
    [JsonPropertyName("speaker_label")] public string? SpeakerLabel { get; set; }
}

public class SpeakerWordType
{
    [JsonPropertyName("segment_id")] public int? SegmentId { get; set; }
    [JsonPropertyName("word_id")] public int? WordId { get; set; }
    [JsonPropertyName("word_type")] public string? WordType { get; set; }
    [JsonPropertyName("confidence")] public double? Confidence { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("start_time")] public double? StartTime { get; set; }
    [JsonPropertyName("end_time")] public double? EndTime { get; set; }
}