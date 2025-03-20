// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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