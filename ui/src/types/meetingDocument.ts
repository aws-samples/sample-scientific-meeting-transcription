/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */


export interface MeetingDocumentRequestType {
  description?: string;
  filename?: string;
}

export interface MeetingDocumentResponseType {
  id?: string;
  meeting_id?: string;
  team_id?: string;
  description?: string;
  filename?: string;
  mimetype?: string;
  created_at: string;
  updated_at: string;
}