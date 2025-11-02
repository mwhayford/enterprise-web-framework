// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { AuthenticatedLayout } from '../components/layout/AuthenticatedLayout'
import { DashboardStats } from '../components/dashboard/DashboardStats'
import { ActivityFeed } from '../components/dashboard/ActivityFeed'
import { QuickActions } from '../components/dashboard/QuickActions'
import {
  dashboardService,
  type DashboardData,
} from '../services/dashboardService'

const DashboardPage: React.FC = () => {
  const { user } = useAuth()
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setLoading(true)
        setError(null)
        // Try real API first, fallback to mock data
        try {
          const data = await dashboardService.getDashboardData()
          setDashboardData(data)
        } catch {
          // Fallback to mock data if API not available
          const data = await dashboardService.getMockDashboardData()
          setDashboardData(data)
        }
      } catch (err) {
        console.error('Failed to load dashboard data:', err)
        setError('Failed to load dashboard data')
        // Use mock data as fallback
        const data = await dashboardService.getMockDashboardData()
        setDashboardData(data)
      } finally {
        setLoading(false)
      }
    }

    if (user) {
      loadDashboardData()
    }
  }, [user])

  if (!user) {
    return null
  }

  if (loading) {
    return (
      <AuthenticatedLayout>
        <div className="flex items-center justify-center h-64">
          <div className="text-gray-600">Loading dashboard...</div>
        </div>
      </AuthenticatedLayout>
    )
  }

  return (
    <AuthenticatedLayout>
      <div className="space-y-6">
        {/* Welcome Section */}
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Welcome back, {user.firstName}!
          </h1>
          <p className="text-gray-600 mt-2">
            Here's what's happening with your rentals today.
          </p>
        </div>

        {/* Stats Cards */}
        {dashboardData && <DashboardStats stats={dashboardData.stats} />}

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Activity Feed */}
          <div className="lg:col-span-2">
            {dashboardData && (
              <ActivityFeed activities={dashboardData.recentActivities} />
            )}
          </div>

          {/* Quick Actions */}
          <div className="lg:col-span-1">
            <QuickActions />
          </div>
        </div>

        {error && (
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-yellow-800">
            <p>{error}</p>
          </div>
        )}
      </div>
    </AuthenticatedLayout>
  )
}

export default DashboardPage
