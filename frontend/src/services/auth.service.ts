/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

// Import authentication type definitions
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

// Import configured axios instance for API calls
import axiosInstance from '../plugins/axios';

// Import AWS Amplify authentication functions
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

/**
 * Service class for handling authentication operations
 * Provides methods for user login, registration, password management, etc.
 */
export class AuthService {
  /**
   * Authenticates a user with Cognito
   * @param credentials - User login credentials (username and password)
   * @returns Promise resolving to boolean indicating success
   */
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
      
      // Get the authentication session with tokens
      const session = await fetchAuthSession();
      
      // Set the Authorization header for all future API calls
      axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${session.tokens?.idToken?.toString()}`;
      return true;
    } catch (error) {
      console.error('Error signing in:', error);
      return false;
    }
  }

  /**
   * Registers a new user with Cognito
   * @param data - User registration data (username, password, email)
   * @returns Promise that resolves when registration is successful
   */
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

  /**
   * Initiates the forgot password flow
   * @param data - Object containing the username
   * @returns Promise that resolves when the reset code is sent
   */
  static async forgotPassword(data: CognitoForgotPasswordRequest): Promise<void> {
    try {
      await resetPassword({ username: data.username });
    } catch (error) {
      throw error;
    }
  }

  /**
   * Confirms a user's registration with verification code
   * @param data - Object containing username and confirmation code
   * @returns Promise that resolves when confirmation is successful
   */
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

  /**
   * Completes the password reset process
   * @param data - Object containing username, confirmation code, and new password
   * @returns Promise that resolves when password is reset
   */
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

  /**
   * Changes the password for an authenticated user
   * @param data - Object containing old and new passwords
   * @returns Promise that resolves when password is changed
   */
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

  /**
   * Resends the verification code for registration
   * @param data - Object containing the username
   * @returns Promise that resolves when code is sent
   */
  static async resendCode(data: CognitoResendCodeRequest): Promise<void> {
    try {
      await resendSignUpCode({
        username: data.username
      });
    } catch (error) {
      throw error;
    }
  }

  /**
   * Logs out the current user
   * @returns Promise that resolves when logout is complete
   */
  static async logout(): Promise<void> {
    try {
      await signOut();
      // Remove the Authorization header after logout
      delete axiosInstance.defaults.headers.common['Authorization'];
    } catch (error) {
      throw error;
    }
  }
}
