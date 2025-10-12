// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { Elements } from '@stripe/react-stripe-js'
import { stripePromise, STRIPE_CONFIG } from '../../config/stripe'

interface StripeProviderProps {
  children: React.ReactNode
  clientSecret?: string
}

export const StripeProvider: React.FC<StripeProviderProps> = ({
  children,
  clientSecret,
}) => {
  const options = {
    clientSecret,
    appearance: STRIPE_CONFIG.appearance,
    ...STRIPE_CONFIG.options,
  }

  return (
    <Elements stripe={stripePromise} options={options}>
      {children}
    </Elements>
  )
}
