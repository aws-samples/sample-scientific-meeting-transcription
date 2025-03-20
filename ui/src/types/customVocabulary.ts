/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { StatusEnum } from './common';

export interface CustomVocabularyRequest {
  vocabulary_name?: string;
  description?: string;
  language_code?: string;
  status: StatusEnum;
}

export interface CustomVocabularyResponse {
  id: string;
  vocabulary_name?: string;
  description?: string;
  language_code?: string;
  status: StatusEnum;
  created_at: string;
  updated_at: string;
  current_step: CustomVocabularyProgressEnum;
  publish_error: string;
  can_perform?: CustomVocabularyCanPerformEnum[];
}

export interface VocabularyPhraseRequest {
  phrase?: string;
  display_as?: string;
  status: StatusEnum;
}

export interface VocabularyPhraseResponse {
  id: string;
  custom_vocabulary_id?: string;
  phrase?: string;
  display_as?: string;
  status: StatusEnum;
  created_at: string;
  updated_at: string;
}

export enum CustomVocabularyProgressEnum {
  Created = 'Created',
  Publishing = 'Publishing',
  Published = 'Published',
  PublishFailed = 'PublishFailed'
}

export enum CustomVocabularyCanPerformEnum {
  Edit = 'Edit',
  Delete = 'Delete',
  Publish = 'Publish',
  Nothing = 'Nothing',
  EditPhrases = 'EditPhrases'
}