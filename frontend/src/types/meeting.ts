/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import { StatusEnum } from './common';
import { CustomModelResponse } from './customModel';
import { CustomVocabularyResponse } from './customVocabulary';
import { MeetingDocumentResponseType } from './meetingDocument';
import { PromptSetResponse } from './promptSet';
import { TeamResponse } from './team';



export interface MeetingRequest {
  title?: string;
  description?: string;
  date?: string;
  current_step?: MeetingSetupProgressEnum;
  prompt_set_id?: string;
  custom_model_id?: string;
  custom_vocabulary_id?: string;
  status?: StatusEnum;
  meeting_notes?: string;
  include_in_model_training?: boolean;
  meeting_notes_version?: number;
}

export interface MeetingResponse {
  id: string;
  prompt_set_id?: string;
  title?: string;
  current_step?: MeetingSetupProgressEnum;
  description?: string;
  team: TeamResponse;
  custom_model_id?: string;
  custom_vocabulary_id?: string;
  custom_vocabulary?: CustomVocabularyResponse;
  custom_model: CustomModelResponse;
  date: string;
  status: StatusEnum;
  created_at: string;
  updated_at: string;
  promptset: PromptSetResponse;
  pre_signed_url?: string;
  transcribe_error?: string;
  meeting_notes?: string;
  include_in_model_training?: boolean;
  meeting_notes_version?: number;
  meeting_notes_version_location?: string;
  can_perform: CanPerformEnum[];
  meeting_documents: MeetingDocumentResponseType[];
}

export interface MeetingPromptResponse {
  prompt_id: string;
  prompt?: string;
  prompt_response_id?: string;
  prompt_response?: MeetingSetupProgressEnum;
  created_at: string;
  updated_at: string;
}

export enum MeetingSetupProgressEnum {
  Created = 'Created',
  Uploaded = 'Uploaded',
  Transcribing = 'Transcribing',
  Transcribed = 'Transcribed',
  TranscribeFailed = 'TranscribeFailed',
  NotesReviewed = 'NotesReviewed',
  Completed = 'Completed',
  PromptProcessing = 'PromptProcessing',
  PromptsFailed = 'PromptsFailed',
  PromptProcessed = 'PromptProcessed',
  Sealed = 'Sealed',
  Sealing = 'Sealing',
  SealFailed = 'SealFailed'
}

export enum CanPerformEnum {
  Upload = 'Upload',
  Nothing = 'Nothing',
  Transcribe = 'Transcribe',
  GenerateNotes = 'GenerateNotes',
  Seal = 'Seal',
  Edit = 'Edit',
  Delete = 'Delete',
  ReviewNotes = 'ReviewNotes',
  ViewPromptNotes = 'ViewPromptNotes',
  ExportNotes = 'ExportNotes',
  ViewAnalytics = 'ViewAnalytics',
  ViewDocuments = 'ViewDocuments'
}

export interface S3PresignedUrlsType {
  download_link: string;
  upload_link: string;
  recording_link: string;
}
