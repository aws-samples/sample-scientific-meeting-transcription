/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

export interface CognitoUser {
  username: string;
  pool: {
    userPoolId: string;
    clientId: string;
  };
  Session?: string | null;
  client: any;
  signInUserSession: {
    idToken: {
      jwtToken: string;
    };
  };
}

export interface CognitoSession {
  getIdToken(): {
    getJwtToken(): string;
  };
}