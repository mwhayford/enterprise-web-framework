// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, CreditCard, FileText, Search } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/Card'
import { Button } from '../ui/Button'

export const QuickActions: React.FC = () => {
  const navigate = useNavigate()

  const actions = [
    {
      label: 'Apply to Property',
      icon: Plus,
      onClick: () => navigate('/properties'),
      variant: 'primary' as const,
    },
    {
      label: 'Make Payment',
      icon: CreditCard,
      onClick: () => navigate('/payment'),
      variant: 'outline' as const,
    },
    {
      label: 'View Applications',
      icon: FileText,
      onClick: () => navigate('/applications/my'),
      variant: 'outline' as const,
    },
    {
      label: 'Search Properties',
      icon: Search,
      onClick: () => navigate('/properties'),
      variant: 'outline' as const,
    },
  ]

  return (
    <Card>
      <CardHeader>
        <CardTitle>Quick Actions</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          {actions.map((action, index) => {
            const Icon = action.icon
            return (
              <Button
                key={index}
                variant={action.variant}
                onClick={action.onClick}
                className="justify-start gap-2"
              >
                <Icon className="w-4 h-4" />
                {action.label}
              </Button>
            )
          })}
        </div>
      </CardContent>
    </Card>
  )
}
