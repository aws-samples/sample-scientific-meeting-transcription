// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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