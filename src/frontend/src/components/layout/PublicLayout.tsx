// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import type { ReactNode } from 'react'
import { PublicNavigation } from '../navigation/PublicNavigation'
import { PublicFooter } from '../navigation/PublicFooter'

interface PublicLayoutProps {
  children: ReactNode
}

export const PublicLayout: React.FC<PublicLayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <PublicNavigation />
      <main className="flex-1 pt-20">
        {children}
      </main>
      <PublicFooter />
    </div>
  )
}

