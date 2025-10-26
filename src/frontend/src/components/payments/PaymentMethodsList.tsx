// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState, useEffect } from 'react'
import { Button } from '../ui/Button'
import { paymentService } from '../../services/paymentService'
import type { PaymentMethod } from '../../types'

interface PaymentMethodsListProps {
  onSelectPaymentMethod?: (paymentMethod: PaymentMethod) => void
  showAddButton?: boolean
}

export const PaymentMethodsList: React.FC<PaymentMethodsListProps> = ({
  onSelectPaymentMethod,
  showAddButton = true,
}) => {
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadPaymentMethods()
  }, [])

  const loadPaymentMethods = async () => {
    try {
      setIsLoading(true)
      const methods = await paymentService.getPaymentMethods()
      setPaymentMethods(methods)
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to load payment methods'
      )
    } finally {
      setIsLoading(false)
    }
  }

  const handleSetDefault = async (paymentMethodId: string) => {
    try {
      await paymentService.setDefaultPaymentMethod(paymentMethodId)
      await loadPaymentMethods() // Reload to update the UI
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Failed to set default payment method'
      )
    }
  }

  const handleDelete = async (paymentMethodId: string) => {
    if (!confirm('Are you sure you want to delete this payment method?')) {
      return
    }

    try {
      await paymentService.deletePaymentMethod(paymentMethodId)
      await loadPaymentMethods() // Reload to update the UI
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete payment method'
      )
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
          <div className="h-20 bg-gray-200 rounded"></div>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md">
        <p className="text-sm text-red-600">{error}</p>
        <Button
          onClick={loadPaymentMethods}
          variant="secondary"
          className="mt-2"
        >
          Retry
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Payment Methods</h3>
        {showAddButton && (
          <Button variant="secondary" size="sm">
            Add Payment Method
          </Button>
        )}
      </div>

      {paymentMethods.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <p>No payment methods found.</p>
          {showAddButton && (
            <Button variant="secondary" className="mt-2">
              Add Your First Payment Method
            </Button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {paymentMethods.map(method => (
            <div
              key={method.id}
              className={`p-4 border rounded-lg cursor-pointer transition-colors ${
                method.isActive
                  ? 'border-gray-200 hover:border-gray-300'
                  : 'border-gray-100 bg-gray-50'
              }`}
              onClick={() => onSelectPaymentMethod?.(method)}
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center space-x-2">
                    <span className="font-medium text-gray-900">
                      {method.displayName}
                    </span>
                    {method.isActive && (
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        Default
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-500 mt-1">
                    {method.type} •••• {method.last4}
                  </p>
                </div>

                <div className="flex space-x-2">
                  {!method.isActive && (
                    <Button
                      variant="secondary"
                      size="sm"
                      onClick={e => {
                        e.stopPropagation()
                        handleSetDefault(method.id)
                      }}
                    >
                      Set Default
                    </Button>
                  )}
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={e => {
                      e.stopPropagation()
                      handleDelete(method.id)
                    }}
                  >
                    Delete
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
