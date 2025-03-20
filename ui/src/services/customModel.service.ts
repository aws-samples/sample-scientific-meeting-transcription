/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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