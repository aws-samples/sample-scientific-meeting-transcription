/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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