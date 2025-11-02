// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { Link } from 'react-router-dom'
import { FileText, Calendar, CreditCard, Bell, ArrowRight } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/Card'
import { cn } from '../../utils/cn'

export interface ActivityItem {
  id: string
  type: 'application' | 'lease' | 'payment' | 'system'
  title: string
  description: string
  timestamp: string
  link?: string
}

interface ActivityFeedProps {
  activities: ActivityItem[]
}

const getActivityIcon = (type: ActivityItem['type']) => {
  switch (type) {
    case 'application':
      return FileText
    case 'lease':
      return Calendar
    case 'payment':
      return CreditCard
    case 'system':
      return Bell
    default:
      return Bell
  }
}

const getActivityColor = (type: ActivityItem['type']) => {
  switch (type) {
    case 'application':
      return 'bg-blue-100 text-blue-600'
    case 'lease':
      return 'bg-green-100 text-green-600'
    case 'payment':
      return 'bg-yellow-100 text-yellow-600'
    case 'system':
      return 'bg-purple-100 text-purple-600'
    default:
      return 'bg-gray-100 text-gray-600'
  }
}

const formatTimestamp = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000)

  if (diffInSeconds < 60) {
    return 'Just now'
  }
  if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60)
    return `${minutes} minute${minutes > 1 ? 's' : ''} ago`
  }
  if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600)
    return `${hours} hour${hours > 1 ? 's' : ''} ago`
  }
  if (diffInSeconds < 604800) {
    const days = Math.floor(diffInSeconds / 86400)
    return `${days} day${days > 1 ? 's' : ''} ago`
  }
  return date.toLocaleDateString()
}

export const ActivityFeed: React.FC<ActivityFeedProps> = ({ activities }) => {
  if (activities.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Recent Activity</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-gray-500 text-center py-8">No recent activity</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Recent Activity</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {activities.map(activity => {
            const Icon = getActivityIcon(activity.type)
            const color = getActivityColor(activity.type)
            const content = (
              <div className="flex items-start gap-4 p-3 rounded-lg hover:bg-gray-50 transition-colors">
                <div className={cn('p-2 rounded-lg flex-shrink-0', color)}>
                  <Icon className="w-5 h-5" />
                </div>
                <div className="flex-1 min-w-0">
                  <h4 className="font-semibold text-gray-900 text-sm">
                    {activity.title}
                  </h4>
                  <p className="text-sm text-gray-600 mt-1">
                    {activity.description}
                  </p>
                  <p className="text-xs text-gray-500 mt-2">
                    {formatTimestamp(activity.timestamp)}
                  </p>
                </div>
                {activity.link && (
                  <ArrowRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
                )}
              </div>
            )

            if (activity.link) {
              return (
                <Link key={activity.id} to={activity.link}>
                  {content}
                </Link>
              )
            }

            return <div key={activity.id}>{content}</div>
          })}
        </div>
      </CardContent>
    </Card>
  )
}

