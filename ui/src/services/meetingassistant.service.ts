/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import axiosInstance from '../plugins/axios';
import { ChatbotRequest, ChatbotResponse, ModelType } from '@/src/types/meetingassistant';

export class ChatbotService {
  static async submitQuestion(teamId: string, data: ChatbotRequest): Promise<ChatbotResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/chatbot`, data);
    return response.data as ChatbotResponse;
  }
  static async getModels(): Promise<ModelType[]> {
    const response = await axiosInstance.get(`/teams/models`);
    return response.data as ModelType[];
  }
}