/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { StatusEnum } from './common';

export interface PromptSetRequest {
  idp_group?: string;
  prompt_set_name?: string;
  description?: string;
  status: StatusEnum;
  create_prompts_from_description: boolean
}

export interface PromptSetResponse {
  id: string;
  prompt_set_name: string;
  description?: string;
  idp_group?: string;
  status: StatusEnum;
  created_at: string;
  updated_at: string;
  create_prompts_from_description: boolean
}

export interface PromptRequest {
  prompt_set_id?: string;
  prompt?: string;
  description?: string;
  status: StatusEnum;
}

export interface PromptResponse {
  id: string;
  order: number;
  prompt_set_id?: string;
  prompt?: string;
  description?: string;
  status: StatusEnum;
  created_at: string;
  updated_at: string;
}