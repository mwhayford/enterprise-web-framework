// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { FileText, Calendar, CreditCard, Clock } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/Card'
import { cn } from '../../utils/cn'

interface DashboardStatsProps {
  stats: {
    activeApplicationsCount: number
    activeLeasesCount: number
    pendingPaymentsCount: number
    upcomingRenewalsCount: number
  }
}

const statCards = [
  {
    key: 'activeApplicationsCount' as const,
    label: 'Active Applications',
    icon: FileText,
    color: 'bg-blue-100 text-blue-600',
    href: '/applications/my',
  },
  {
    key: 'activeLeasesCount' as const,
    label: 'Active Leases',
    icon: Calendar,
    color: 'bg-green-100 text-green-600',
    href: '/leases',
  },
  {
    key: 'pendingPaymentsCount' as const,
    label: 'Pending Payments',
    icon: CreditCard,
    color: 'bg-yellow-100 text-yellow-600',
    href: '/payment',
  },
  {
    key: 'upcomingRenewalsCount' as const,
    label: 'Upcoming Renewals',
    icon: Clock,
    color: 'bg-purple-100 text-purple-600',
    href: '/leases',
  },
]

export const DashboardStats: React.FC<DashboardStatsProps> = ({ stats }) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {statCards.map(card => {
        const Icon = card.icon
        const value = stats[card.key]
        return (
          <Card
            key={card.key}
            className="hover:shadow-md transition-shadow cursor-pointer"
          >
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">
                {card.label}
              </CardTitle>
              <div className={cn('p-2 rounded-lg', card.color)}>
                <Icon className="w-5 h-5" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-gray-900">{value}</div>
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
