/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
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