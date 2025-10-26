// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Button } from '../components/ui/Button'
import { Navigation } from '../components/landing/Navigation'
import { PricingCard } from '../components/landing/PricingCard'
import { TestimonialsSection } from '../components/landing/TestimonialsSection'
import { FAQSection } from '../components/landing/FAQSection'
import { ScreenshotGallery } from '../components/landing/ScreenshotGallery'
import { NewsletterSignup } from '../components/landing/NewsletterSignup'

const LandingPage: React.FC = () => {
  const navigate = useNavigate()
  const [isAnnual, setIsAnnual] = useState(false)

  const handleGetStarted = () => {
    navigate('/login')
  }

  const pricingPlans = [
    {
      name: 'Starter',
      price: '29',
      annualPrice: '23',
      description: 'Perfect for small landlords getting started',
      features: [
        { text: 'Up to 10 properties', included: true },
        { text: 'Unlimited tenants', included: true },
        { text: 'Online payment processing', included: true },
        { text: 'Maintenance tracking', included: true },
        { text: 'Basic reports', included: true },
        { text: 'Email support', included: true },
        { text: 'Advanced analytics', included: false },
        { text: 'API access', included: false },
        { text: 'Priority support', included: false },
      ],
    },
    {
      name: 'Professional',
      price: '79',
      annualPrice: '63',
      description: 'For growing property management businesses',
      features: [
        { text: 'Up to 50 properties', included: true },
        { text: 'Unlimited tenants', included: true },
        { text: 'Online payment processing', included: true },
        { text: 'Maintenance tracking', included: true },
        { text: 'Advanced analytics', included: true },
        { text: 'Custom reports', included: true },
        { text: 'API access', included: true },
        { text: 'Priority email support', included: true },
        { text: 'Dedicated account manager', included: false },
      ],
      isPopular: true,
    },
    {
      name: 'Enterprise',
      price: 'Custom',
      description: 'For large portfolios with custom needs',
      features: [
        { text: 'Unlimited properties', included: true },
        { text: 'Unlimited tenants', included: true },
        { text: 'Online payment processing', included: true },
        { text: 'Maintenance tracking', included: true },
        { text: 'Advanced analytics', included: true },
        { text: 'Custom integrations', included: true },
        { text: 'Full API access', included: true },
        { text: 'Dedicated account manager', included: true },
        { text: '24/7 phone support', included: true },
      ],
    },
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50">
      {/* Navigation */}
      <Navigation onGetStarted={handleGetStarted} />

      {/* Hero Section */}
      <section className="relative pt-32 pb-20 sm:pt-40 sm:pb-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <motion.h1
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6 }}
              className="text-4xl sm:text-6xl lg:text-7xl font-extrabold text-gray-900 tracking-tight"
            >
              Simplify Your
              <span className="block bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                Rental Management
              </span>
            </motion.h1>
            <motion.p
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
              className="mt-6 text-xl sm:text-2xl text-gray-600 max-w-3xl mx-auto"
            >
              The all-in-one platform to manage properties, tenants, payments,
              and maintenance with ease.
            </motion.p>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.4 }}
              className="mt-10 flex flex-col sm:flex-row gap-4 justify-center"
            >
              <Button
                size="lg"
                onClick={handleGetStarted}
                className="text-lg px-8 py-6 shadow-lg hover:shadow-xl transition-shadow"
              >
                Start Free Trial
              </Button>
              <Button
                size="lg"
                variant="outline"
                className="text-lg px-8 py-6"
                onClick={() => {
                  document
                    .getElementById('features')
                    ?.scrollIntoView({ behavior: 'smooth' })
                }}
              >
                Learn More
              </Button>
            </motion.div>
            <motion.p
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.6, delay: 0.6 }}
              className="mt-4 text-sm text-gray-500"
            >
              No credit card required • Free 14-day trial
            </motion.p>
          </div>
        </div>

        {/* Decorative elements */}
        <div className="absolute top-0 left-0 w-full h-full overflow-hidden -z-10">
          <div className="absolute top-20 -left-20 w-72 h-72 bg-purple-300 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob"></div>
          <div className="absolute top-40 -right-20 w-72 h-72 bg-blue-300 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob animation-delay-2000"></div>
          <div className="absolute -bottom-8 left-1/2 w-72 h-72 bg-pink-300 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob animation-delay-4000"></div>
        </div>
      </section>

      {/* Stats Section */}
      <motion.section
        initial={{ opacity: 0, y: 20 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, margin: "-100px" }}
        transition={{ duration: 0.6 }}
        className="py-12 bg-white/50 backdrop-blur-sm"
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: 0.1 }}
              className="text-center"
            >
              <p className="text-4xl font-bold text-gray-900">10k+</p>
              <p className="mt-2 text-sm text-gray-600">Properties Managed</p>
            </motion.div>
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: 0.2 }}
              className="text-center"
            >
              <p className="text-4xl font-bold text-gray-900">50k+</p>
              <p className="mt-2 text-sm text-gray-600">Active Tenants</p>
            </motion.div>
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: 0.3 }}
              className="text-center"
            >
              <p className="text-4xl font-bold text-gray-900">$2M+</p>
              <p className="mt-2 text-sm text-gray-600">Monthly Payments</p>
            </motion.div>
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: 0.4 }}
              className="text-center"
            >
              <p className="text-4xl font-bold text-gray-900">99.9%</p>
              <p className="mt-2 text-sm text-gray-600">Uptime</p>
            </motion.div>
          </div>
        </div>
      </motion.section>

      {/* Features Section */}
      <section id="features" className="py-20 sm:py-32">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true, margin: "-100px" }}
            transition={{ duration: 0.6 }}
            className="text-center"
          >
            <h2 className="text-3xl sm:text-5xl font-bold text-gray-900">
              Everything you need to manage rentals
            </h2>
            <p className="mt-4 text-xl text-gray-600 max-w-2xl mx-auto">
              Powerful features designed to make property management effortless
            </p>
          </motion.div>

          <div className="mt-16 grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
            {/* Feature 1 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.1 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-blue-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Property Management
                </h3>
                <p className="text-gray-600">
                  Manage unlimited properties with detailed profiles, photos, and
                  documents all in one place.
                </p>
              </div>
            </motion.div>

            {/* Feature 2 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.2 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-purple-600 to-pink-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-purple-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Online Payments
                </h3>
                <p className="text-gray-600">
                  Accept rent payments online with Stripe. Automated reminders and
                  instant payment confirmations.
                </p>
              </div>
            </motion.div>

            {/* Feature 3 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.3 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-pink-600 to-orange-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-pink-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-pink-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Tenant Portal
                </h3>
                <p className="text-gray-600">
                  Give tenants access to pay rent, submit maintenance requests, and
                  view their lease information.
                </p>
              </div>
            </motion.div>

            {/* Feature 4 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.4 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-green-600 to-teal-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-green-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Maintenance Tracking
                </h3>
                <p className="text-gray-600">
                  Track maintenance requests, assign tasks, and keep a complete
                  history of all repairs and updates.
                </p>
              </div>
            </motion.div>

            {/* Feature 5 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.5 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-indigo-600 to-blue-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-indigo-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Analytics & Reports
                </h3>
                <p className="text-gray-600">
                  Get insights into your rental business with comprehensive reports
                  and real-time analytics.
                </p>
              </div>
            </motion.div>

            {/* Feature 6 */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: 0.6 }}
              className="relative group"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-yellow-600 to-orange-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
              <div className="relative bg-white p-8 rounded-xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-yellow-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                    />
                  </svg>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  Smart Search
                </h3>
                <p className="text-gray-600">
                  Powered by Elasticsearch, quickly find properties, tenants, and
                  documents with lightning-fast search.
                </p>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Pricing Section */}
      <section id="pricing" className="py-20 sm:py-32 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Section Header */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true, margin: "-100px" }}
            transition={{ duration: 0.6 }}
            className="text-center mb-16"
          >
            <h2 className="text-3xl sm:text-5xl font-bold text-gray-900 mb-4">
              Simple, transparent pricing
            </h2>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto mb-8">
              Choose the plan that fits your portfolio size. Cancel anytime.
            </p>

            {/* Billing Toggle */}
            <div className="flex items-center justify-center gap-4">
              <span
                className={`text-sm font-medium ${
                  !isAnnual ? 'text-gray-900' : 'text-gray-500'
                }`}
              >
                Monthly
              </span>
              {/* eslint-disable-next-line jsx-a11y/aria-props */}
              <button
                onClick={() => setIsAnnual(!isAnnual)}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                  isAnnual ? 'bg-blue-600' : 'bg-gray-300'
                }`}
                type="button"
                role="switch"
                aria-checked={isAnnual}
                aria-label="Toggle annual billing"
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    isAnnual ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
              <span
                className={`text-sm font-medium ${
                  isAnnual ? 'text-gray-900' : 'text-gray-500'
                }`}
              >
                Annual{' '}
                <span className="text-green-600 font-semibold">(Save 20%)</span>
              </span>
            </div>
          </motion.div>

          {/* Pricing Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mt-12">
            {pricingPlans.map((plan, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.5, delay: index * 0.1 }}
              >
                <PricingCard
                key={index}
                name={plan.name}
                price={plan.price}
                annualPrice={plan.annualPrice}
                description={plan.description}
                features={plan.features}
                isPopular={plan.isPopular}
                isAnnual={isAnnual}
                onSelect={handleGetStarted}
              />
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Testimonials Section */}
      <section id="testimonials">
        <TestimonialsSection />
      </section>

      {/* Screenshot Gallery */}
      <ScreenshotGallery />

      {/* FAQ Section */}
      <FAQSection />

      {/* Newsletter Section */}
      <NewsletterSignup />

      {/* Final CTA Section */}
      <section className="relative py-20 sm:py-32 bg-gradient-to-r from-blue-600 to-purple-600">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl sm:text-5xl font-bold text-white">
              Ready to simplify your rental management?
            </h2>
            <p className="mt-6 text-xl text-blue-100 max-w-2xl mx-auto">
              Join thousands of property managers who trust RentalManager to run
              their business.
            </p>
            <div className="mt-10">
              <Button
                size="lg"
                variant="outline"
                onClick={handleGetStarted}
                className="bg-white text-blue-600 hover:bg-blue-50 text-lg px-8 py-6 shadow-xl"
              >
                Get Started Free
              </Button>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-400 py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h3 className="text-2xl font-bold text-white mb-4">RentalManager</h3>
            <p className="text-sm">
              © 2025 RentalManager. All rights reserved.
            </p>
            <div className="mt-6 flex justify-center gap-6">
              <a href="#" className="hover:text-white transition-colors">
                Privacy Policy
              </a>
              <a href="#" className="hover:text-white transition-colors">
                Terms of Service
              </a>
              <a href="#" className="hover:text-white transition-colors">
                Contact
              </a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}

export default LandingPage

