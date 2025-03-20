/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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