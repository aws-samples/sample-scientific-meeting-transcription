/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

export interface CognitoLoginRequest {
  username: string;
  password: string;
}

export interface CognitoRegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface CognitoForgotPasswordRequest {
  username: string;
}

export interface CognitoConfirmSignUpRequest {
  username: string;
  code: string;
}

export interface CognitoResetPasswordRequest {
  username: string;
  code: string;
  password: string;
  newPassword: string;
  oldPassword: string;
}

export interface CognitoChangePasswordRequest {
  newPassword: string;
  oldPassword: string;
  
}

export interface CognitoResendCodeRequest {
  username: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export interface CognitoLoginResponse {
  accessToken?: string;
  expiresIn?: number;
  idToken?: string;
  newDeviceMetadata?: string;
  refreshToken?: string;
  tokenType?: string;
}