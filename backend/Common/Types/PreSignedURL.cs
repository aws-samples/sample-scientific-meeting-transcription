// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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