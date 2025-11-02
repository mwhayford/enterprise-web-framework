// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { Button } from '../ui/Button'

interface PricingFeature {
  text: string
  included: boolean
}

interface PricingCardProps {
  name: string
  price: string
  annualPrice?: string
  description: string
  features: PricingFeature[]
  isPopular?: boolean
  isAnnual: boolean
  onSelect: () => void
  ctaText?: string
}

export const PricingCard: React.FC<PricingCardProps> = ({
  name,
  price,
  annualPrice,
  description,
  features,
  isPopular = false,
  isAnnual,
  onSelect,
  ctaText = 'Get Started',
}) => {
  const displayPrice = isAnnual && annualPrice ? annualPrice : price

  return (
    <div className="relative">
      {isPopular && (
        <div className="absolute -top-5 left-0 right-0 flex justify-center">
          <span className="bg-gradient-to-r from-blue-600 to-purple-600 text-white px-4 py-1 rounded-full text-sm font-semibold">
            Most Popular
          </span>
        </div>
      )}
      <div
        className={`relative h-full bg-white rounded-2xl shadow-lg p-8 ${
          isPopular
            ? 'ring-2 ring-blue-600 shadow-xl scale-105'
            : 'hover:shadow-xl'
        } transition-all duration-200`}
      >
        <div className="mb-8">
          <h3 className="text-2xl font-bold text-gray-900 mb-2">{name}</h3>
          <p className="text-gray-600 text-sm">{description}</p>
        </div>

        <div className="mb-8">
          {displayPrice === 'Custom' ? (
            <div className="text-4xl font-bold text-gray-900">
              Contact Sales
            </div>
          ) : (
            <>
              <div className="flex items-baseline">
                <span className="text-5xl font-bold text-gray-900">
                  ${displayPrice}
                </span>
                <span className="text-gray-600 ml-2">
                  /mo{isAnnual && ' billed annually'}
                </span>
              </div>
              {isAnnual && annualPrice && (
                <p className="text-sm text-green-600 mt-2">
                  Save $
                  {(
                    parseFloat(price) * 12 -
                    parseFloat(annualPrice) * 12
                  ).toFixed(0)}
                  /year
                </p>
              )}
            </>
          )}
        </div>

        <Button
          className={`w-full mb-8 ${
            isPopular
              ? 'bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700'
              : ''
          }`}
          size="lg"
          onClick={onSelect}
        >
          {ctaText}
        </Button>

        <div className="space-y-4">
          {features.map((feature, index) => (
            <div key={index} className="flex items-start">
              {feature.included ? (
                <svg
                  className="w-5 h-5 text-green-500 mt-0.5 mr-3 flex-shrink-0"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              ) : (
                <svg
                  className="w-5 h-5 text-gray-300 mt-0.5 mr-3 flex-shrink-0"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              )}
              <span
                className={feature.included ? 'text-gray-700' : 'text-gray-400'}
              >
                {feature.text}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
