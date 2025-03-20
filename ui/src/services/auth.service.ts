/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import {
  CognitoLoginRequest,
  CognitoRegisterRequest,
  CognitoForgotPasswordRequest,
  CognitoConfirmSignUpRequest,
  CognitoResetPasswordRequest,
  CognitoChangePasswordRequest,
  CognitoResendCodeRequest,
  CognitoLoginResponse
} from '@/src/types/auth';

import axiosInstance from '../plugins/axios';

import { Amplify } from 'aws-amplify';
import { 
  signIn, 
  signUp, 
  confirmSignUp, 
  resetPassword,
  confirmResetPassword, 
  getCurrentUser, 
  updatePassword, 
  resendSignUpCode,
  signOut,
  fetchUserAttributes,
  fetchAuthSession
} from 'aws-amplify/auth';

export class AuthService {
  static async login(credentials: CognitoLoginRequest): Promise<boolean> {
    try {
      // First, try to sign out any existing user
      try {
        await signOut({ global: true });
      } catch (signOutError) {
        // Ignore sign out errors - user might not be signed in
        console.log('No existing session to clear');
      }
  
      // Now proceed with sign in
      const user = await signIn({
        username: credentials.username,
        password: credentials.password
      });
      
      const session = await fetchAuthSession();
      
      axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${session.tokens?.idToken?.toString()}`;
      return true;
    } catch (error) {
      console.error('Error signing in:', error);
      return false;
    }
  }

  static async register(data: CognitoRegisterRequest): Promise<void> {
    try {
      await signUp({
        username: data.username,
        password: data.password,
        options: {
          userAttributes: {
            email: data.email,
          }
        }
      });
    } catch (error) {
      throw error;
    }
  }

  static async forgotPassword(data: CognitoForgotPasswordRequest): Promise<void> {
    try {
      await resetPassword({ username: data.username });
    } catch (error) {
      throw error;
    }
  }

  static async confirmSignUp(data: CognitoConfirmSignUpRequest): Promise<void> {
    try {
      await confirmSignUp({
        username: data.username,
        confirmationCode: data.code
      });
    } catch (error) {
      throw error;
    }
  }

  static async resetPassword(data: CognitoResetPasswordRequest): Promise<void> {
    try {
      await confirmResetPassword({
        username: data.username,
        confirmationCode: data.code,
        newPassword: data.newPassword
      });
    } catch (error) {
      throw error;
    }
  }

  static async changePassword(data: CognitoChangePasswordRequest): Promise<void> {
    try {
      await updatePassword({
        oldPassword: data.oldPassword,
        newPassword: data.newPassword
      });
    } catch (error) {
      throw error;
    }
  }

  static async resendCode(data: CognitoResendCodeRequest): Promise<void> {
    try {
      await resendSignUpCode({
        username: data.username
      });
    } catch (error) {
      throw error;
    }
  }

  static async logout(): Promise<void> {
    try {
      await signOut();
      delete axiosInstance.defaults.headers.common['Authorization'];
    } catch (error) {
      throw error;
    }
  }
}
