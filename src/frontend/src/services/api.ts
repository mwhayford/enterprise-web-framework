// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import axios from 'axios';
import type { AxiosInstance } from 'axios';
import type {
  User,
  Payment,
  Subscription,
  CreatePaymentRequest,
  CreateSubscriptionRequest,
} from '../types';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111/api',
      timeout: 10000,
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('auth_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor to handle token refresh
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid
          localStorage.removeItem('auth_token');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async googleLogin(): Promise<void> {
    window.location.href = `${this.api.defaults.baseURL}/auth/google`;
  }

  async refreshToken(): Promise<{ token: string }> {
    const response = await this.api.post('/auth/refresh');
    return response.data;
  }

  async logout(): Promise<void> {
    await this.api.post('/auth/logout');
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
  }

  // User endpoints
  async getCurrentUser(): Promise<User> {
    const response = await this.api.get('/users/me');
    return response.data;
  }

  async updateUserProfile(data: Partial<User>): Promise<User> {
    const response = await this.api.put('/users/me', data);
    return response.data;
  }

  // Payment endpoints
  async processPayment(data: CreatePaymentRequest): Promise<Payment> {
    const response = await this.api.post('/payments/process', data);
    return response.data;
  }

  async getPaymentHistory(page = 1, pageSize = 20): Promise<Payment[]> {
    const response = await this.api.get(`/payments/history?page=${page}&pageSize=${pageSize}`);
    return response.data;
  }

  // Subscription endpoints
  async createSubscription(data: CreateSubscriptionRequest): Promise<Subscription> {
    const response = await this.api.post('/subscriptions', data);
    return response.data;
  }

  async getSubscriptions(): Promise<Subscription[]> {
    const response = await this.api.get('/subscriptions');
    return response.data;
  }

  // Utility methods
  setAuthToken(token: string): void {
    localStorage.setItem('auth_token', token);
    this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  removeAuthToken(): void {
    localStorage.removeItem('auth_token');
    delete this.api.defaults.headers.common['Authorization'];
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  }

  // Search endpoints
  async search<T = any>(query: string, index: string, page: number, pageSize: number): Promise<T> {
    const response = await this.api.get('/search', {
      params: { query, index, page, pageSize }
    });
    return response.data;
  }

  async searchUsers(query: string, page: number, pageSize: number): Promise<any> {
    const response = await this.api.get('/search/users', {
      params: { query, page, pageSize }
    });
    return response.data;
  }

  async searchPayments(query: string, page: number, pageSize: number): Promise<any> {
    const response = await this.api.get('/search/payments', {
      params: { query, page, pageSize }
    });
    return response.data;
  }

  async indexDocument(document: any, index: string, id?: string): Promise<void> {
    await this.api.post('/search/index', document, {
      params: { index, id }
    });
  }
}

export const apiService = new ApiService();
export default apiService;
