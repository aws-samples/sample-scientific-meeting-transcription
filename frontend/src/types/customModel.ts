/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import { StatusEnum, TranscribeBaseModel } from './common';

export interface CustomModelRequest {
  model_name?: string;
  description?: string;
  base_model_name?: string;
  status: StatusEnum;
  language_code?: string;
  transcribe_base_model_name: TranscribeBaseModel;
}

export interface CustomModelResponse {
  id: string;
  model_name?: string;
  description?: string;
  language_code?: string;
  status: StatusEnum;
  created_at?: string;
  updated_at?: string;
  custom_model_progress_status: CustomModelSetupProgressEnum;
  presigned_url?: string;
  transcribe_base_model_name: TranscribeBaseModel;
  presigned_url_expire?: string;
  can_perform?: CustomModelCanPerformEnum[];
}

export enum CustomModelSetupProgressEnum {
  Created = 'Created',
  S3SignedUrlCreated = 'S3SignedUrlCreated',
  TrainingDataUploaded = 'TrainingDataUploaded',
  TrainingQueued = 'TrainingQueued',
  TrainingStarted = 'TrainingStarted',
  ModelReady = 'ModelReady',
  TrainingFailed = 'TrainingFailed'
}

export enum CustomModelCanPerformEnum {
  Edit = 'Edit',
  Delete = 'Delete',
  Train = 'Train',
  Upload = 'Upload',
  Nothing = 'Nothing'
}