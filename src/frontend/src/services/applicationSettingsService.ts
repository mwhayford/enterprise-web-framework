import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111';

export interface ApplicationSettingsDto {
  id: string;
  defaultApplicationFee: number;
  defaultApplicationFeeCurrency: string;
  applicationFeeEnabled: boolean;
  requirePaymentUpfront: boolean;
  maxApplicationsPerUser?: number;
  applicationFormFields?: string;
  updatedBy?: string;
  updatedAt: string;
}

export const applicationSettingsService = {
  async getSettings(): Promise<ApplicationSettingsDto> {
    const response = await axios.get(`${API_BASE_URL}/api/applicationsettings`);
    return response.data;
  },
};

