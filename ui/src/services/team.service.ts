/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { TeamRequest, TeamResponse } from '@/src/types/team';
import { PaginatedList } from '@/src/types/common';
import axiosInstance from '../plugins/axios';


export class TeamService {
  static async getTeams(pageIndex = 1, pageSize = 10): Promise<PaginatedList<TeamResponse>> {
    const response = await axiosInstance.get('/teams', {
      params: { pageIndex, pageSize }
    });
    return response.data;
  }

  static async getTeam(teamId: string): Promise<TeamResponse> {
    const response = await axiosInstance.get(`/teams/${teamId}`);
    return response.data;
  }

  static async createTeam(data: TeamRequest): Promise<TeamResponse> {
    const response = await axiosInstance.post('/teams', data);
    return response.data;
  }

  static async updateTeam(teamId: string, data: TeamRequest): Promise<TeamResponse> {
    const response = await axiosInstance.put(`/teams/${teamId}`, data);
    return response.data;
  }

  static async deleteTeam(teamId: string): Promise<void> {
    await axiosInstance.delete(`/teams/${teamId}`);
  }
}