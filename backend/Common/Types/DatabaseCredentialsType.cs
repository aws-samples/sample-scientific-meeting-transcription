// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json.Serialization;

namespace Common.Types
{
    public class DatabaseCredentialsType
    {
        [JsonPropertyName("password")] public string? Password { get; set; }

        [JsonPropertyName("dbname")] public string? DbName { get; set; }

        [JsonPropertyName("engine")] public string? Engine { get; set; }

        [JsonPropertyName("port")] public int? Port { get; set; }

        [JsonPropertyName("dbInstanceIdentifier")]
        public string? DbInstanceIdentifier { get; set; }

        [JsonPropertyName("host")] public string? Host { get; set; }

        [JsonPropertyName("username")] public string? Username { get; set; }

        public string GetConnectionString()
        {
            return $"Host={Host};Database={DbName};Username={Username};Password={Password};";
        }
    }
}