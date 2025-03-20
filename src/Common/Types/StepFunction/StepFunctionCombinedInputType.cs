// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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