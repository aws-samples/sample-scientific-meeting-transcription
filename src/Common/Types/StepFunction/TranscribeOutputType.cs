// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Types.StepFunction
{
    public class TranscribeOutputType
    {
        [JsonPropertyName("jobName")] public string? jobName { get; set; }

        [JsonPropertyName("accountId")] public string? accountId { get; set; }

        [JsonPropertyName("status")] public string? status { get; set; }

        [JsonPropertyName("results")] public Results? results { get; set; }
    }

    public class Results
    {
        [JsonPropertyName("transcripts")] public List<Transcripts>? transcripts { get; set; }

        [JsonPropertyName("speaker_labels")] public Speaker_labels? speaker_labels { get; set; }

        [JsonPropertyName("items")] public List<ResultItemsType?>? items { get; set; }

        [JsonPropertyName("audio_segments")] public List<Audio_segments>? audio_segments { get; set; }
    }

    public class Transcripts
    {
        [JsonPropertyName("transcript")] public string? transcript { get; set; }
    }

    public class Speaker_labels
    {
        [JsonPropertyName("segments")] public List<Segments>? segments { get; set; }

        [JsonPropertyName("channel_label")] public string? channel_label { get; set; }

        [JsonPropertyName("speakers")] public int? speakers { get; set; }
    }

    public class Segments
    {
        [JsonPropertyName("start_time")] public string? start_time { get; set; }

        [JsonPropertyName("end_time")] public string? end_time { get; set; }

        [JsonPropertyName("speaker_label")] public string? speaker_label { get; set; }

        [JsonPropertyName("items")] public List<SegmentItemType>? items { get; set; }
    }

    public class SegmentItemType
    {
        [JsonPropertyName("speaker_label")] public string? speaker_label { get; set; }

        [JsonPropertyName("start_time")] public string? start_time { get; set; }

        [JsonPropertyName("end_time")] public string? end_time { get; set; }
    }

    public class ResultItemsType
    {
        [JsonPropertyName("id")] public int? id { get; set; }

        [JsonPropertyName("type")] public string? type { get; set; }

        [JsonPropertyName("alternatives")] public List<Alternatives>? alternatives { get; set; }

        [JsonPropertyName("start_time")] public string? start_time { get; set; }

        [JsonPropertyName("end_time")] public string? end_time { get; set; }

        [JsonPropertyName("speaker_label")] public string? speaker_label { get; set; }
    }

    public class Alternatives
    {
        [JsonPropertyName("confidence")] public double? confidence { get; set; }

        [JsonPropertyName("content")] public string? content { get; set; }
    }

    public class Audio_segments
    {
        [JsonPropertyName("id")] public int? id { get; set; }

        [JsonPropertyName("transcript")] public string? transcript { get; set; }

        [JsonPropertyName("start_time")] public string? start_time { get; set; }

        [JsonPropertyName("end_time")] public string? end_time { get; set; }

        [JsonPropertyName("speaker_label")] public string? speaker_label { get; set; }

        [JsonPropertyName("items")] public List<int>? items { get; set; }
    }
}