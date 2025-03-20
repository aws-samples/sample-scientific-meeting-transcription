import axios from 'axios';
import axiosInstance from '../plugins/axios';

export interface VersionInfo {
  version: string;
  buildDate: string;
}

class VersionService {
  private static instance: VersionService;
  private versionInfo: VersionInfo | null = null;

  private constructor() {}

  public static getInstance(): VersionService {
    if (!VersionService.instance) {
      VersionService.instance = new VersionService();
    }
    return VersionService.instance;
  }

  async getVersion(): Promise<VersionInfo> {
    if (this.versionInfo) {
      return this.versionInfo;
    }

    try {
      // First try to get version from API
      const response = await axiosInstance.get('/teams/version');
      this.versionInfo = response.data;
      return this.versionInfo;
    } catch (error) {
      try {
        // If API fails, try to get version from local file
        const response = await axios.get('/version.json');
        this.versionInfo = response.data;
        return this.versionInfo;
      } catch (fallbackError) {
        console.error('Failed to fetch version info:', fallbackError);
        // Fallback to hardcoded version if both methods fail
        return { version: '1.0.0', buildDate: new Date().toISOString().split('T')[0] };
      }
    }
  }
}

export default VersionService.getInstance();
