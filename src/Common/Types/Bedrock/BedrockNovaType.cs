// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;

namespace Common.Types.Bedrock
{
    public class BedrockNovaType
    {
        public string? schemaVersion { get; set; } = "messages-v1";
        public List<NovaMessageType>? messages { get; set; }
        public List<NovaSystemType>? system { get; set; }
        public InferenceConfigType? inferenceConfig { get; set; }

        public class NovaSystemType
        {
            public string? text { get; set; }
        }

        public class NovaMessageType
        {
            public string? role { get; set; }
            public List<ContentType>? content { get; set; }
        }

        public class ContentType
        {
            public string? text { get; set; }
        }

        public class InferenceConfigType
        {
            public double? temperature { get; set; } = 0.7;
            public double? top_k { get; set; } = 20;
            public double? top_p { get; set; } = 0.9;
            public int? max_new_tokens { get; set; }
        }

        // Helper method to create a request with a single user message
        public static BedrockNovaType CreateUserRequest(string prompt, string systemMessage)
        {
            return new BedrockNovaType
            {
                system = new List<NovaSystemType>()
                {
                    new() { text = systemMessage }
                },
                schemaVersion = "messages-v1",
                messages =
                [
                    new NovaMessageType
                    {
                        role = "user",
                        content = [new ContentType() { text = prompt }]
                    }
                ],
                inferenceConfig = new InferenceConfigType
                {
                    temperature = 0.7,
                    top_p = 0.9,
                    max_new_tokens = 5000
                }
            };
        }
    }
}