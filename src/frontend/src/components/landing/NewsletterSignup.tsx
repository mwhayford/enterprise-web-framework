// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState } from 'react'
import { Button } from '../ui/Button'
import { subscribeToNewsletter } from '../../services/newsletterService'

export const NewsletterSignup: React.FC = () => {
  const [email, setEmail] = useState('')
  const [consent, setConsent] = useState(false)
  const [status, setStatus] = useState<
    'idle' | 'loading' | 'success' | 'error'
  >('idle')
  const [errorMessage, setErrorMessage] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!email || !consent) {
      setStatus('error')
      setErrorMessage('Please provide your email and consent to continue.')
      return
    }

    setStatus('loading')
    setErrorMessage('')

    try {
      await subscribeToNewsletter(email)
      setStatus('success')
      setEmail('')
      setConsent(false)
    } catch (error) {
      setStatus('error')
      setErrorMessage(
        error instanceof Error
          ? error.message
          : 'Failed to subscribe. Please try again.'
      )
    }
  }

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value)
    if (status === 'error') {
      setStatus('idle')
      setErrorMessage('')
    }
  }

  const handleConsentChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setConsent(e.target.checked)
    if (status === 'error') {
      setStatus('idle')
      setErrorMessage('')
    }
  }

  return (
    <section className="py-20 sm:py-32 bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-white rounded-2xl shadow-xl p-8 sm:p-12">
          {/* Header */}
          <div className="text-center mb-8">
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
              Stay up to date
            </h2>
            <p className="text-lg text-gray-600">
              Get the latest product updates, tips, and exclusive offers
              delivered to your inbox.
            </p>
          </div>

          {/* Form or Success Message */}
          {status === 'success' ? (
            <div className="text-center py-8">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg
                  className="w-8 h-8 text-green-600"
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
              </div>
              <h3 className="text-2xl font-semibold text-gray-900 mb-2">
                Thank you for subscribing!
              </h3>
              <p className="text-gray-600">
                Check your email to confirm your subscription.
              </p>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Email Input */}
              <div>
                <label htmlFor="newsletter-email" className="sr-only">
                  Email address
                </label>
                <input
                  id="newsletter-email"
                  type="email"
                  value={email}
                  onChange={handleEmailChange}
                  placeholder="Enter your email address"
                  className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  required
                  disabled={status === 'loading'}
                />
              </div>

              {/* Consent Checkbox */}
              <div className="flex items-start">
                <input
                  id="newsletter-consent"
                  type="checkbox"
                  checked={consent}
                  onChange={handleConsentChange}
                  className="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  required
                  disabled={status === 'loading'}
                />
                <label
                  htmlFor="newsletter-consent"
                  className="ml-3 text-sm text-gray-600"
                >
                  I agree to receive marketing emails from RentalManager. You
                  can unsubscribe at any time. View our{' '}
                  <a href="#" className="text-blue-600 hover:text-blue-700">
                    Privacy Policy
                  </a>
                  .
                </label>
              </div>

              {/* Error Message */}
              {status === 'error' && errorMessage && (
                <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-600">{errorMessage}</p>
                </div>
              )}

              {/* Submit Button */}
              <Button
                type="submit"
                size="lg"
                className="w-full"
                disabled={status === 'loading'}
              >
                {status === 'loading'
                  ? 'Subscribing...'
                  : 'Subscribe to Newsletter'}
              </Button>

              {/* Privacy Note */}
              <p className="text-xs text-gray-500 text-center">
                We respect your privacy. No spam, ever.
              </p>
            </form>
          )}
        </div>
      </div>
    </section>
  )
}
