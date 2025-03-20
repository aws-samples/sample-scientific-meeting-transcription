/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { MeetingDocumentRequestType, MeetingDocumentResponseType } from '@/src/types/meetingDocument';
import { PaginatedList } from '@/src/types/common';
import axiosInstance from '../plugins/axios';

export class MeetingDocumentsService {
  static async listMeetingDocuments(teamId: string, meetingId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<MeetingDocumentResponseType>> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/documents`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async createMeetingDocument(teamId: string, meetingId: string, data: MeetingDocumentRequestType): Promise<MeetingDocumentResponseType> {
    const response = await axiosInstance.post(`/teams/${teamId}/meetings/${meetingId}/documents`, data);
    return response.data;
  }

  static async deleteMeetingDocument(teamId: string, meetingId: string, documentId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/meetings/${meetingId}/documents/${documentId}`);
  }

  static async getMeetingDocumentUploadUrl(teamId: string, meetingId: string, documentId: string): Promise<string> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/documents/${documentId}/document_upload_url`);
    return response.data.url;
  }
}