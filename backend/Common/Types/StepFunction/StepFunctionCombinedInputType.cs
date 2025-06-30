// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json.Serialization;

namespace Common.Types.StepFunction
{
    public class StepFunctionCombinedInputType
    {
        [JsonPropertyName("step_function_job_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StepFunctionJobType StepFunctionJobType { get; set; }

        [JsonPropertyName("transcribe")] public TranscriptionMeetingInputType? TranscribeInput { get; set; }

        [JsonPropertyName("promptprocess")] public PromptProcessLambdaInputType? PromptProcessInput { get; set; }

        [JsonPropertyName("custommodel")] public CustomModelStepMachineType? CustomModelInput { get; set; }
        [JsonPropertyName("customvocabulary")] public CustomVocabularyStepMachineType? CustomVocabularyInput { get; set; }
        [JsonPropertyName("sealmeeting")] public SealMeetingStepMachineType? SealMeetingInput { get; set; }
    }

    public enum StepFunctionJobType
    {
        Transcribe,
        PromptProcess,
        CustomModel,
        CustomVocabulary,
        SealMeeting
    }
}