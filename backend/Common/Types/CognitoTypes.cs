// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json.Serialization;

namespace Common.Types
{
    public class CognitoLoginRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("password")] public string? Password { get; set; }
    }

    public class CognitoRegisterRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("password")] public string? Password { get; set; }
    }

    public class CognitoConfirmSignUpRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("code")] public string? Code { get; set; }
    }

    public class CognitoResendCodeRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
    }

    public class CognitoForgotPasswordRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
    }

    public class ConitoResetPasswordRequestType
    {
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("code")] public string? Code { get; set; }
        [JsonPropertyName("password")] public string? Password { get; set; }
    }

    public class CognitoChangePasswordRequestType
    {
        [JsonPropertyName("old_password")] public string? OldPassword { get; set; }
        [JsonPropertyName("new_password")] public string? NewPassword { get; set; }
    }
}