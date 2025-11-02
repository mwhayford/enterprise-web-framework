// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import axios from 'axios'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111'

export interface DashboardStats {
  activeApplicationsCount: number
  activeLeasesCount: number
  pendingPaymentsCount: number
  upcomingRenewalsCount: number
}

export interface ActivityItem {
  id: string
  type: 'application' | 'lease' | 'payment' | 'system'
  title: string
  description: string
  timestamp: string
  link?: string
}

export interface DashboardData {
  stats: DashboardStats
  recentActivities: ActivityItem[]
}

export const dashboardService = {
  async getDashboardData(): Promise<DashboardData> {
    // For now, return mock data. This will be replaced with actual API calls
    // when backend endpoints are available
    const response = await axios.get(`${API_BASE_URL}/api/dashboard`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
      },
    })
    return response.data
  },

  // Mock data for development
  async getMockDashboardData(): Promise<DashboardData> {
    return {
      stats: {
        activeApplicationsCount: 3,
        activeLeasesCount: 2,
        pendingPaymentsCount: 1,
        upcomingRenewalsCount: 0,
      },
      recentActivities: [
        {
          id: '1',
          type: 'application',
          title: 'Application Status Updated',
          description: 'Your application for Property #123 has been approved',
          timestamp: new Date().toISOString(),
          link: '/applications/my',
        },
        {
          id: '2',
          type: 'payment',
          title: 'Payment Received',
          description: 'Payment of $1,200.00 was successfully processed',
          timestamp: new Date(Date.now() - 86400000).toISOString(),
          link: '/payment',
        },
        {
          id: '3',
          type: 'lease',
          title: 'Lease Renewal Reminder',
          description: 'Your lease for Property #456 expires in 30 days',
          timestamp: new Date(Date.now() - 172800000).toISOString(),
          link: '/leases',
        },
      ],
    }
  },
}
