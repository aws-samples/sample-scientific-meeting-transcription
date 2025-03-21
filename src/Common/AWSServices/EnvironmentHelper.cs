// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;

namespace Common.AWSServices;

public static class EnvironmentHelper
{
    private static string? GetEnvironmentVariable(string envVariable)
    {
        string? returnValue = Environment.GetEnvironmentVariable(envVariable);
        if (returnValue == null)
        {
            throw new Exception($"{envVariable} environment variable is not set");
        }

        return returnValue;
    }

    public static string? DB_SECRET_KEY => GetEnvironmentVariable("DB_SECRET_KEY");

    public static string? S3_UPLOAD_BUCKET_PARAM_ID => GetEnvironmentVariable("S3_UPLOAD_BUCKET_PARAM_ID");

    public static string? TRANSCRIBE_STATEMACHINE_ARN => GetEnvironmentVariable("TRANSCRIBE_STATEMACHINE_ARN");

    public static string? CUSTOMMODEL_STATEMACHINE_ARN => GetEnvironmentVariable("CUSTOMMODEL_STATEMACHINE_ARN");

    public static string? CUSTOMVOCABULARY_STATEMACHINE_ARN => GetEnvironmentVariable("CUSTOMVOCABULARY_STATEMACHINE_ARN");

    public static string? PROMPT_STATEMACHINE_ARN => GetEnvironmentVariable("PROMPT_STATEMACHINE_ARN");

    public static string? BEDROCK_KB_ID => GetEnvironmentVariable("BEDROCK_KB_ID");

    public static string? BEDROCK_KB_DATASOURCE_ID => GetEnvironmentVariable("BEDROCK_KB_DATASOURCE_ID");

    public static string? SEALMEETING_STATEMACHINE_ARN => GetEnvironmentVariable("SEALMEETING_STATEMACHINE_ARN");

    public static string? BEDROCK_MODEL_INFERENCE_ARN => GetEnvironmentVariable("BEDROCK_MODEL_INFERENCE_ARN");
}