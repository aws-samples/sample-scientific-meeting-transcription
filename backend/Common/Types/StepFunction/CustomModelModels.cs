// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Text.Json.Serialization;
using Common.Types.CustomModels;

namespace Common.Types.StepFunction
{
    public class CustomModelStepMachineType
    {
        [JsonPropertyName("modelStatus")] public ModelStatus? ModelStatus { get; set; }
        [JsonPropertyName("custommodel")] public CustomModelResponseType? CustomModel { get; set; }

        [JsonPropertyName("id")] public Guid? Id { get; set; }

        [JsonPropertyName("custom_model_progress_status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomModelSetupProgressEnum? ModelSetupProgress { get; set; }

        [JsonPropertyName("model_setup_message")]
        public StepFunctionErrorType? SetupError { get; set; }
    }


    public class ModelStatus
    {
        [JsonPropertyName("LanguageModel")] public LanguageModel? LanguageModel { get; set; }
    }

    public class LanguageModel
    {
        [JsonPropertyName("BaseModelName")] public string? BaseModelName { get; set; }

        [JsonPropertyName("CreateTime")] public string? CreateTime { get; set; }

        [JsonPropertyName("LanguageCode")] public string? LanguageCode { get; set; }

        [JsonPropertyName("LastModifiedTime")] public string? LastModifiedTime { get; set; }

        [JsonPropertyName("ModelName")] public string? ModelName { get; set; }

        [JsonPropertyName("ModelStatus")] public string? ModelStatus { get; set; }

        [JsonPropertyName("UpgradeAvailability")]
        public bool UpgradeAvailability { get; set; }
    }
}