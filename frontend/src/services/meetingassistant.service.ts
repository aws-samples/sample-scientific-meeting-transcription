/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
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