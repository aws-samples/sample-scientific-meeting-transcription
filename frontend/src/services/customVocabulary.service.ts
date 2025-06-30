/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import { CustomVocabularyRequest, CustomVocabularyResponse, VocabularyPhraseRequest, VocabularyPhraseResponse } from '@/src/types/customVocabulary';
import { PaginatedList } from '@/src/types/common';
import axiosInstance from '../plugins/axios';


export class CustomVocabularyService {
  static async getCustomVocabularies(teamId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<CustomVocabularyResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/customvocabularies`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getCustomVocabulary(teamId: string, customVocabolaryId: string): Promise<CustomVocabularyResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/customvocabularies/${customVocabolaryId}`);
    return response.data;
  }

  static async createCustomVocabulary(teamId: string, data: CustomVocabularyRequest): Promise<CustomVocabularyResponse> {
    const response = await axiosInstance.post(`/teams/${teamId}/customvocabularies`, data);
    return response.data;
  }

  static async updateCustomVocabulary(teamId: string, customVocabolaryId: string, data: CustomVocabularyRequest): Promise<CustomVocabularyResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/customvocabularies/${customVocabolaryId}`, data);
    return response.data;
  }

  static async deleteCustomVocabulary(teamId: string, customVocabolaryId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/customvocabularies/${customVocabolaryId}`);
  }
  
  static async publishCustomVocabulary(teamId: string, customVocabolaryId: string): Promise<void> {
    await axiosInstance.put(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/publish`);
  }

  // VocabularyPhrases
  static async getVocabularyPhrases(teamId: string, customVocabolaryId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<VocabularyPhraseResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/vocabularyphrases`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getVocabularyPhrase(teamId: string, customVocabolaryId: string, vocabularypraseId: string): Promise<VocabularyPhraseResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/vocabularyphrases/${vocabularypraseId}`);
    return response.data;
  }

  static async createVocabularyPhrase(teamId: string, customVocabolaryId: string, data: VocabularyPhraseRequest): Promise<VocabularyPhraseResponse> {
    const response = await axiosInstance.post(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/vocabularyphrases`, data);
    return response.data;
  }

  static async updateVocabularyPhrase(teamId: string, customVocabolaryId: string, vocabularypraseId: string, data: VocabularyPhraseRequest): Promise<VocabularyPhraseResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/vocabularyphrases/${vocabularypraseId}`, data);
    return response.data;
  }

  static async deleteVocabularyPhrase(teamId: string, customVocabolaryId: string, vocabularypraseId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/customvocabularies/${customVocabolaryId}/vocabularyphrases/${vocabularypraseId}`);
  }
}