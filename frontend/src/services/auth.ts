import { apiRequest } from './api';

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  companyId: string;
  companyName: string;
}

export const authService = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    return apiRequest<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  },

  register: async (payload: any): Promise<LoginResponse> => {
    return apiRequest<LoginResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  getProfile: async (companyId: string, userId: string): Promise<UserProfile> => {
    return apiRequest<UserProfile>(`/api/companies/${companyId}/users/${userId}`);
  },
};
