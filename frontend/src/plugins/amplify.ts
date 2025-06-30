/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

// Import AWS Amplify library for authentication and AWS service integration
import { Amplify } from 'aws-amplify';
import { cognitoUserPoolsTokenProvider } from 'aws-amplify/auth/cognito';

Amplify.configure({
  Auth: {
    Cognito: {
      // Client ID for the Cognito User Pool application client
      userPoolClientId: import.meta.env.VITE_COGNITO_CLIENT_ID,
      
      // ID of the Cognito User Pool for user authentication
      userPoolId: import.meta.env.VITE_COGNITO_USER_POOL_ID,
      
      // ID of the Cognito Identity Pool for AWS service authorization
      identityPoolId: import.meta.env.VITE_COGNITO_IDENTITY_POOL_ID,
      
      // Configure login options - allow username and email login, disable phone
      loginWith: {
        username: true,
        email: true,
        phone: false,
        oauth: {
          domain: import.meta.env.VITE_COGNITO_DOMAIN || 'your-domain.auth.us-east-1.amazoncognito.com',
          scopes: ['openid', 'email', 'profile'],
          redirectSignIn: [import.meta.env.VITE_REDIRECT_SIGN_IN || 'http://localhost:3000/'],
          redirectSignOut: [import.meta.env.VITE_REDIRECT_SIGN_OUT || 'http://localhost:3000/'],
          responseType: 'code'
        }
      }
    }
  }
});

// Configure the token provider to handle OAuth redirects
cognitoUserPoolsTokenProvider.handleSignInRedirect();