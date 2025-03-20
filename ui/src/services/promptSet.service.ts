/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import axios from 'axios';
import { PromptSetRequest, PromptSetResponse, PromptRequest, PromptResponse } from '@/src/types/promptSet';
import { PaginatedList } from '@/src/types/common';
import axiosInstance from '../plugins/axios';
import { messageType } from '@/src/types/message';


export class PromptSetService {
  static async getPromptSets(teamId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<PromptSetResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/promptSets`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getPromptSet(teamId: string, promptSetId: string): Promise<PromptSetResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/promptSets/${promptSetId}`);
    return response.data;
  }

  static async createPromptSet(teamId: string, data: PromptSetRequest): Promise<PromptSetResponse> {
    const response = await axiosInstance.post(`/teams/${teamId}/promptSets`, data);
    return response.data;
  }

  static async updatePromptSet(teamId: string, promptSetId: string, data: PromptSetRequest): Promise<PromptSetResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/promptSets/${promptSetId}`, data);
    return response.data;
  }

  static async deletePromptSet(teamId: string, promptSetId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/promptSets/${promptSetId}`);
  }

  // Prompts
  static async getPrompts(teamId: string, promptSetId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<PromptResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/promptsets/${promptSetId}/prompts`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getPrompt(teamId: string, promptSetId: string, promptId: string): Promise<PromptResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/promptsets/${promptSetId}/prompts/${promptId}`);
    return response.data;
  }

  static async createPrompt(teamId: string, promptSetId: string, data: PromptRequest): Promise<PromptResponse> {
    const response = await axiosInstance.post(`/teams/${teamId}/promptsets/${promptSetId}/prompts`, data);
    return response.data;
  }

  static async updatePrompt(teamId: string, promptSetId: string, promptId: string, data: PromptRequest): Promise<PromptResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/promptsets/${promptSetId}/prompts/${promptId}`, data);
    return response.data;
  }

  static async deletePrompt(teamId: string, promptSetId: string, promptId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/promptsets/${promptSetId}/prompts/${promptId}`);
  }
  static async moveUpPrompt(teamId: string, promptSetId: string, promptId: string): Promise<messageType> {
    const response =await axiosInstance.put(`/teams/${teamId}/promptsets/${promptSetId}/prompts/${promptId}/moveup`);
    return response.data;
  }
  static async moveDownPrompt(teamId: string, promptSetId: string, promptId: string): Promise<messageType> {
    const response = await axiosInstance.put(`/teams/${teamId}/promptsets/${promptSetId}/prompts/${promptId}/movedown`);
    return response.data;
  }
}