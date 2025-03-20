// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using Common.Types.StepFunction;

namespace StepFunctionLambda.Services;

public interface IPromptProcessService
{
    Task<object> ProcessPromptRequest(PromptProcessLambdaInputType input);
}