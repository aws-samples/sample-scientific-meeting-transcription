// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;

namespace Common.Types.StepFunction
{
    public class CustomVocabularyStepMachineType
    {
        [JsonPropertyName("id")] public Guid? Id { get; set; }
    }
}