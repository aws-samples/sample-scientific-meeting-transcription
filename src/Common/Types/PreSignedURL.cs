// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;

namespace Common.Types;

public class PresignedUrlRequest
{
    public string? BucketName { get; set; }
    public string? ObjectKey { get; set; }
    public bool IsUpload { get; set; }
    public int ExpirationMinutes { get; set; } = 60;
    public string? ContentType { get; set; }
}

public class PresignedUrlResponse
{
    [JsonPropertyName("pre_signed_url")] public string? PreSignedUrl { get; set; }
    [JsonPropertyName("expiration")] public DateTime? Expiration { get; set; }
    [JsonPropertyName("full_path")] public string? FullPath { get; set; }
}