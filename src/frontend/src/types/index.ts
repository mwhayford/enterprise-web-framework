// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName?: string;
  avatarUrl?: string;
  stripeCustomerId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export type PaymentStatus =
  | 'Pending'
  | 'Processing'
  | 'Succeeded'
  | 'Failed'
  | 'Cancelled'
  | 'Refunded'
  | 'PartiallyRefunded';

export interface Payment {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  paymentMethodType: PaymentMethodType;
  stripePaymentIntentId?: string;
  description?: string;
  createdAt: string;
  processedAt?: string;
}

export type PaymentMethodType =
  | 'Card'
  | 'Ach'
  | 'BankTransfer'
  | 'Cash';

export interface PaymentMethod {
  id: string;
  userId: string;
  type: PaymentMethodType;
  last4?: string;
  externalId: string;
  isActive: boolean;
  createdAt: string;
  displayName: string;
}

export type SubscriptionStatus =
  | 'Incomplete'
  | 'IncompleteExpired'
  | 'Trialing'
  | 'Active'
  | 'PastDue'
  | 'Canceled'
  | 'Unpaid'
  | 'Paused';

export interface Subscription {
  id: string;
  userId: string;
  planId: string;
  amount: number;
  currency: string;
  status: SubscriptionStatus;
  stripeSubscriptionId?: string;
  currentPeriodStart?: string;
  currentPeriodEnd?: string;
  trialStart?: string;
  trialEnd?: string;
  canceledAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
}

export interface ErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
}

export interface SearchResult<T> {
  documents: T[];
  totalHits: number;
  page: number;
  pageSize: number;
  totalPages: number;
  maxScore: number;
  took: number;
  aggregations: Record<string, any>;
}

export interface CreatePaymentRequest {
  amount: number;
  currency: string;
  paymentMethodId: string;
  description?: string;
}

export interface CreateSubscriptionRequest {
  planId: string;
  paymentMethodId: string;
}

// Payment form types
export interface PaymentFormData {
  amount: number;
  currency: string;
  description?: string;
  paymentMethodType: PaymentMethodType;
}

export interface SubscriptionFormData {
  planId: string;
  amount: number;
  currency: string;
  paymentMethodType: PaymentMethodType;
}

// Stripe Elements types
export interface StripeCardElement {
  mount(domElement: string | HTMLElement): void;
  unmount(): void;
  destroy(): void;
  on(event: string, handler: (event: any) => void): void;
  focus(): void;
  blur(): void;
  clear(): void;
}

export interface StripePaymentElement {
  mount(domElement: string | HTMLElement): void;
  unmount(): void;
  destroy(): void;
  on(event: string, handler: (event: any) => void): void;
  focus(): void;
  blur(): void;
  clear(): void;
}
