// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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