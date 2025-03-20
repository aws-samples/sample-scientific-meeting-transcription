/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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