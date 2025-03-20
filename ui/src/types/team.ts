/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { StatusEnum } from './common';

export interface TeamRequest {
  team?: string;
  status: StatusEnum;
  idp_group?: string;
}

export interface TeamResponse {
  id?: string;
  team?: string;
  status: StatusEnum;
  idp_group?: string;
  created_at?: string;
  updated_at?: string;
}