/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import axios from 'axios';
import { MeetingPromptResponse, MeetingRequest, MeetingResponse, MeetingSetupProgressEnum, S3PresignedUrlsType } from '@/src/types/meeting';
import { PaginatedList } from '@/src/types/common';
import  axiosInstance from '../plugins/axios';

export class MeetingService {
  static async getMeetings(teamId: string, pageIndex = 1, pageSize = 10): Promise<PaginatedList<MeetingResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings`, {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getMeeting(teamId: string, meetingId: string): Promise<MeetingResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}`);
    return response.data;
  }

  static async createMeeting(teamId: string, data: MeetingRequest): Promise<MeetingResponse> {
    data.current_step = MeetingSetupProgressEnum.Created;
    const response = await axiosInstance.post(`/teams/${teamId}/meetings`, data);
    return response.data;
  }

  static async updateMeeting(teamId: string, meetingId: string, data: MeetingRequest): Promise<MeetingResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/meetings/${meetingId}`, data);
    return response.data;
  }

  static async deleteMeeting(teamId: string, meetingId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}/meetings/${meetingId}`);
  }
  

  static async createSignedUrl(teamId: string, meetingId: string): Promise<MeetingResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/create_signed_url`);
    return response.data;
  }

  static async startTranscriptionProcessing(teamId: string, meetingId: string): Promise<MeetingResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/meetings/${meetingId}/start_transcription_processing`);
    return response.data;
  }
  static async startPromptProcessing(teamId: string, meetingId: string): Promise<MeetingResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}/meetings/${meetingId}/start_prompt_processing`);
    return response.data;
  }
  static async GetPromptResponses(teamId: string, meetingId: string): Promise<PaginatedList<MeetingPromptResponse>> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/prompt_responses`);
    return response.data;
  }
  static async sealMeeting(teamId: string, meetingId: string): Promise<MeetingPromptResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/seal_meeting`);
    return response.data;
  }
  static async downloadMeetingNotesURL(teamId: string, meetingId: string): Promise<S3PresignedUrlsType> {
    const preSignedUrlResponse = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/meeting_notes_urls`);
    return preSignedUrlResponse.data;
  }
  static async downloadMeetingAnalytics(teamId: string, meetingId: string): Promise<string> {
    const analyticsResponse = await axiosInstance.get(`/teams/${teamId}/meetings/${meetingId}/meeting_analytics`);
    return analyticsResponse.data;
  }
}