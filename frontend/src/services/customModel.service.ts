/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import { CustomModelRequest, CustomModelResponse } from '@/src/types/customModel';
import { PaginatedList } from '@/src/types/common';
import axiosInstance from '../plugins/axios';

export class CustomModelService {
  static async getCustomModels(teamId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<CustomModelResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/customModels`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getCustomModel(teamId: string, customModelId: string): Promise<CustomModelResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/customModels/${customModelId}`);
    return response.data;
  }

  static async createCustomModel(teamId: string, data: CustomModelRequest): Promise<CustomModelResponse> {
    const response = await axiosInstance.post(`/teams/${teamId}/customModels`, data);
    return response.data;
  }

  static async updateCustomModel(teamId: string, customModelId: string, data: CustomModelRequest): Promise<CustomModelResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/customModels/${customModelId}`, data);
    return response.data;
  }

  static async deleteCustomModel(teamId: string, customModelId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/customModels/${customModelId}`);
  }

  static async createSignedUrl(teamId: string, customModelId: string): Promise<CustomModelResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/customModels/${customModelId}/create_signed_url`);
    return response.data;
  }

  static async startModelTraining(teamId: string, customModelId: string): Promise<CustomModelResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/customModels/${customModelId}/start_model_training`);
    return response.data;
  }
}