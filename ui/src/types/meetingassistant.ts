/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */


type Nullable<T> = T | null;

export interface ChatbotRequest {
  meeting_id?: Nullable<string>;
  question?: string;
}

export interface ChatbotResponse {
  result: string;
}