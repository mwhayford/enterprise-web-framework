// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Home,
  UserPlus,
  FileText,
  CreditCard,
  CheckCircle,
  Search,
  Calendar,
  Wrench,
  BarChart3,
  Shield,
  Zap,
  Users,
} from 'lucide-react'
import { PublicLayout } from '../components/layout/PublicLayout'
import { Button } from '../components/ui/Button'

const HowItWorksPage: React.FC = () => {
  const navigate = useNavigate()

  const landlordSteps = [
    {
      number: 1,
      icon: Home,
      title: 'Create Your Account',
      description:
        'Sign up with Google OAuth to create your RentalManager account. No credit card required for the free trial.',
      color: 'bg-blue-100 text-blue-600',
    },
    {
      number: 2,
      icon: Home,
      title: 'Add Your Properties',
      description:
        'List your rental properties with photos, descriptions, amenities, and pricing details. Set availability dates and rental terms.',
      color: 'bg-purple-100 text-purple-600',
    },
    {
      number: 3,
      icon: Search,
      title: 'Tenants Find You',
      description:
        'Prospective tenants browse available properties, filter by their preferences, and submit applications directly through the platform.',
      color: 'bg-green-100 text-green-600',
    },
    {
      number: 4,
      icon: FileText,
      title: 'Review Applications',
      description:
        'Review rental applications, check tenant backgrounds, and approve or reject applicants. All application data is organized and secure.',
      color: 'bg-yellow-100 text-yellow-600',
    },
    {
      number: 5,
      icon: CheckCircle,
      title: 'Create Leases',
      description:
        'Generate digital leases for approved tenants. Set lease terms, rent amounts, and automatic renewal reminders.',
      color: 'bg-indigo-100 text-indigo-600',
    },
    {
      number: 6,
      icon: CreditCard,
      title: 'Collect Rent Online',
      description:
        'Tenants pay rent securely through Stripe. Receive automatic payment confirmations and track payment history in real-time.',
      color: 'bg-pink-100 text-pink-600',
    },
  ]

  const tenantSteps = [
    {
      number: 1,
      icon: Search,
      title: 'Browse Properties',
      description:
        'Search and filter available properties by location, price, bedrooms, bathrooms, and amenities. No account needed to browse.',
      color: 'bg-blue-100 text-blue-600',
    },
    {
      number: 2,
      icon: Home,
      title: 'View Property Details',
      description:
        'See detailed property information including photos, amenities, location details, rent, deposits, and application fees.',
      color: 'bg-purple-100 text-purple-600',
    },
    {
      number: 3,
      icon: UserPlus,
      title: 'Sign Up & Apply',
      description:
        'Create an account and fill out the rental application. Provide personal info, employment details, rental history, and references.',
      color: 'bg-green-100 text-green-600',
    },
    {
      number: 4,
      icon: FileText,
      title: 'Track Application Status',
      description:
        'Monitor your application status in real-time. Receive notifications when your application is reviewed, approved, or needs additional information.',
      color: 'bg-yellow-100 text-yellow-600',
    },
    {
      number: 5,
      icon: CheckCircle,
      title: 'Sign Lease & Move In',
      description:
        'If approved, review and sign your lease digitally. Access your lease documents anytime from your dashboard.',
      color: 'bg-indigo-100 text-indigo-600',
    },
    {
      number: 6,
      icon: CreditCard,
      title: 'Pay Rent Online',
      description:
        'Pay rent securely through Stripe. Set up automatic payments, view payment history, and receive instant receipts.',
      color: 'bg-pink-100 text-pink-600',
    },
  ]

  const keyFeatures = [
    {
      icon: Zap,
      title: 'Lightning Fast',
      description: 'Powered by modern technology for instant results and smooth user experience.',
      color: 'bg-yellow-100 text-yellow-600',
    },
    {
      icon: Shield,
      title: 'Secure & Private',
      description: 'Bank-level security with encrypted data and secure payment processing.',
      color: 'bg-blue-100 text-blue-600',
    },
    {
      icon: BarChart3,
      title: 'Smart Analytics',
      description: 'Track your rental performance with comprehensive reports and insights.',
      color: 'bg-green-100 text-green-600',
    },
    {
      icon: Users,
      title: 'Tenant Portal',
      description: 'Give tenants self-service access to pay rent, submit maintenance requests, and view lease info.',
      color: 'bg-purple-100 text-purple-600',
    },
    {
      icon: Wrench,
      title: 'Maintenance Management',
      description: 'Track maintenance requests from submission to completion. Assign contractors and monitor progress.',
      color: 'bg-orange-100 text-orange-600',
    },
    {
      icon: Calendar,
      title: 'Automated Reminders',
      description: 'Never miss a payment or renewal. Automated reminders keep everyone informed.',
      color: 'bg-pink-100 text-pink-600',
    },
  ]

  return (
    <PublicLayout>
      <div className="bg-gradient-to-br from-blue-50 via-white to-purple-50">
        {/* Hero Section */}
        <section className="py-20 sm:py-32">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6 }}
              className="text-center"
            >
              <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold text-gray-900 mb-6">
                How{' '}
                <span className="bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                  RentalManager
                </span>{' '}
                Works
              </h1>
              <p className="text-xl text-gray-600 max-w-3xl mx-auto mb-8">
                Everything you need to know about using RentalManager to simplify
                your rental property management or find your next home.
              </p>
            </motion.div>
          </div>
        </section>

        {/* For Property Owners Section */}
        <section className="py-16 bg-white">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-12">
              <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
                For Property Owners & Landlords
              </h2>
              <p className="text-xl text-gray-600 max-w-2xl mx-auto">
                Manage your entire rental business from one powerful platform
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
              {landlordSteps.map((step, index) => {
                const Icon = step.icon
                return (
                  <motion.div
                    key={step.number}
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: index * 0.1 }}
                    className="relative group"
                  >
                    <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
                    <div className="relative bg-white p-6 rounded-xl shadow-sm hover:shadow-lg transition-shadow h-full">
                      <div className="flex items-start gap-4">
                        <div
                          className={`${step.color} p-3 rounded-lg flex-shrink-0`}
                        >
                          <Icon className="w-6 h-6" />
                        </div>
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <span className="text-2xl font-bold text-gray-400">
                              {step.number}
                            </span>
                            <h3 className="text-lg font-semibold text-gray-900">
                              {step.title}
                            </h3>
                          </div>
                          <p className="text-gray-600 text-sm leading-relaxed">
                            {step.description}
                          </p>
                        </div>
                      </div>
                    </div>
                  </motion.div>
                )
              })}
            </div>
          </div>
        </section>

        {/* For Tenants Section */}
        <section className="py-16 bg-gray-50">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-12">
              <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
                For Tenants & Renters
              </h2>
              <p className="text-xl text-gray-600 max-w-2xl mx-auto">
                Find your perfect rental and manage everything in one place
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
              {tenantSteps.map((step, index) => {
                const Icon = step.icon
                return (
                  <motion.div
                    key={step.number}
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: index * 0.1 }}
                    className="relative group"
                  >
                    <div className="absolute -inset-1 bg-gradient-to-r from-purple-600 to-pink-600 rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-200"></div>
                    <div className="relative bg-white p-6 rounded-xl shadow-sm hover:shadow-lg transition-shadow h-full">
                      <div className="flex items-start gap-4">
                        <div
                          className={`${step.color} p-3 rounded-lg flex-shrink-0`}
                        >
                          <Icon className="w-6 h-6" />
                        </div>
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <span className="text-2xl font-bold text-gray-400">
                              {step.number}
                            </span>
                            <h3 className="text-lg font-semibold text-gray-900">
                              {step.title}
                            </h3>
                          </div>
                          <p className="text-gray-600 text-sm leading-relaxed">
                            {step.description}
                          </p>
                        </div>
                      </div>
                    </div>
                  </motion.div>
                )
              })}
            </div>
          </div>
        </section>

        {/* Key Features Section */}
        <section className="py-16 bg-white">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-12">
              <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
                Why Choose RentalManager?
              </h2>
              <p className="text-xl text-gray-600 max-w-2xl mx-auto">
                Powerful features designed to make rental management effortless
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {keyFeatures.map((feature, index) => {
                const Icon = feature.icon
                return (
                  <motion.div
                    key={feature.title}
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5, delay: index * 0.1 }}
                    className="bg-gray-50 p-6 rounded-xl hover:shadow-md transition-shadow"
                  >
                    <div className={`${feature.color} w-12 h-12 rounded-lg flex items-center justify-center mb-4`}>
                      <Icon className="w-6 h-6" />
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                      {feature.title}
                    </h3>
                    <p className="text-gray-600 text-sm">{feature.description}</p>
                  </motion.div>
                )
              })}
            </div>
          </div>
        </section>

        {/* Process Flow Visualization */}
        <section className="py-16 bg-gradient-to-r from-blue-600 to-purple-600">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-12">
              <h2 className="text-3xl sm:text-4xl font-bold text-white mb-4">
                Simple Process Flow
              </h2>
              <p className="text-xl text-blue-100 max-w-2xl mx-auto">
                From listing to lease in just a few steps
              </p>
            </div>

            <div className="flex flex-wrap justify-center items-center gap-4 md:gap-8 text-white">
              <div className="flex flex-col items-center">
                <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center mb-2">
                  <Home className="w-8 h-8" />
                </div>
                <span className="text-sm font-medium">List Property</span>
              </div>
              <div className="hidden md:block text-2xl text-white/50">→</div>
              <div className="flex flex-col items-center">
                <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center mb-2">
                  <Search className="w-8 h-8" />
                </div>
                <span className="text-sm font-medium">Tenant Finds</span>
              </div>
              <div className="hidden md:block text-2xl text-white/50">→</div>
              <div className="flex flex-col items-center">
                <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center mb-2">
                  <FileText className="w-8 h-8" />
                </div>
                <span className="text-sm font-medium">Application</span>
              </div>
              <div className="hidden md:block text-2xl text-white/50">→</div>
              <div className="flex flex-col items-center">
                <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center mb-2">
                  <CheckCircle className="w-8 h-8" />
                </div>
                <span className="text-sm font-medium">Approved</span>
              </div>
              <div className="hidden md:block text-2xl text-white/50">→</div>
              <div className="flex flex-col items-center">
                <div className="w-16 h-16 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center mb-2">
                  <CreditCard className="w-8 h-8" />
                </div>
                <span className="text-sm font-medium">Pay Rent</span>
              </div>
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section className="py-16 bg-white">
          <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
              Ready to Get Started?
            </h2>
            <p className="text-xl text-gray-600 mb-8">
              Join thousands of property managers and tenants using RentalManager
              to simplify their rental experience.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button
                size="lg"
                onClick={() => navigate('/login')}
                className="text-lg px-8 py-6"
              >
                Start Free Trial
              </Button>
              <Button
                size="lg"
                variant="outline"
                onClick={() => navigate('/properties')}
                className="text-lg px-8 py-6"
              >
                Browse Properties
              </Button>
            </div>
            <p className="mt-6 text-sm text-gray-500">
              No credit card required • Free 14-day trial • Cancel anytime
            </p>
          </div>
        </section>
      </div>
    </PublicLayout>
  )
}

export default HowItWorksPage

