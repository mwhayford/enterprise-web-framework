// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { Link, useLocation } from 'react-router-dom'
import { ChevronRight } from 'lucide-react'
import { cn } from '../../utils/cn'

interface BreadcrumbItem {
  label: string
  path?: string
}

const routeLabels: Record<string, string> = {
  dashboard: 'Dashboard',
  properties: 'Properties',
  applications: 'Applications',
  leases: 'Leases',
  payment: 'Payments',
  search: 'Search',
  profile: 'Profile',
  settings: 'Settings',
  notifications: 'Notifications',
}

export const Breadcrumbs: React.FC = () => {
  const location = useLocation()
  const pathnames = location.pathname.split('/').filter(x => x)

  const breadcrumbs: BreadcrumbItem[] = [
    { label: 'Home', path: '/dashboard' },
  ]

  // Build breadcrumbs from path
  pathnames.forEach((value, index) => {
    const path = `/${pathnames.slice(0, index + 1).join('/')}`
    const label =
      routeLabels[value] ||
      value
        .split('-')
        .map(word => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ')
    breadcrumbs.push({ label, path })
  })

  // Remove duplicates (e.g., "my" in /applications/my)
  const uniqueBreadcrumbs = breadcrumbs.filter(
    (crumb, index, self) =>
      index === self.findIndex(c => c.path === crumb.path)
  )

  if (uniqueBreadcrumbs.length <= 1) {
    return null
  }

  return (
    <nav className="flex" aria-label="Breadcrumb">
      <ol className="flex items-center space-x-2 text-sm text-gray-600">
        {uniqueBreadcrumbs.map((crumb, index) => {
          const isLast = index === uniqueBreadcrumbs.length - 1

          return (
            <li key={crumb.path || 'current'} className="flex items-center">
              {index > 0 && (
                <ChevronRight className="w-4 h-4 mx-2 text-gray-400" />
              )}
              {isLast || !crumb.path ? (
                <span
                  className={cn(
                    'font-medium',
                    isLast ? 'text-gray-900' : 'text-gray-600'
                  )}
                >
                  {crumb.label}
                </span>
              ) : (
                <Link
                  to={crumb.path}
                  className="hover:text-blue-600 transition-colors"
                >
                  {crumb.label}
                </Link>
              )}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}

