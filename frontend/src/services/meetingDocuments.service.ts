/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
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