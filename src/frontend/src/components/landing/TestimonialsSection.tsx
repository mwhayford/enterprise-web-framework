// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { TestimonialCard } from './TestimonialCard'

const testimonials = [
  {
    quote:
      'RentalManager transformed how we handle our properties. What used to take hours now takes minutes. The automated payment reminders alone have saved us countless headaches.',
    author: 'Sarah Johnson',
    role: 'Property Manager',
    company: 'Urban Properties LLC',
    rating: 5,
    metric: 'Saved 10+ hours per week',
  },
  {
    quote:
      "The tenant portal is a game-changer. Our tenants love being able to pay rent online and submit maintenance requests instantly. It's made our operation so much more professional.",
    author: 'Michael Chen',
    role: 'Real Estate Investor',
    company: 'Chen Investments',
    rating: 5,
    metric: '99% on-time payments',
  },
  {
    quote:
      'We manage over 200 units and RentalManager scales perfectly with our needs. The analytics dashboard gives us insights we never had before.',
    author: 'Jennifer Martinez',
    role: 'Portfolio Manager',
    company: 'Skyline Property Group',
    rating: 5,
    metric: '200+ properties managed',
  },
  {
    quote:
      "The search functionality is incredibly fast. Finding documents, payments, and tenant information is now instant. It's saved our team so much time.",
    author: 'David Thompson',
    role: 'Operations Director',
    company: 'Metro Housing Partners',
    rating: 5,
  },
  {
    quote:
      'As a small landlord, I was worried about the learning curve, but RentalManager is so intuitive. I was up and running in less than an hour. Highly recommend!',
    author: 'Emily Rodriguez',
    role: 'Independent Landlord',
    company: 'Self-Employed',
    rating: 5,
    metric: 'Setup in under 1 hour',
  },
  {
    quote:
      'The maintenance tracking feature keeps us organized and our tenants happy. We can see the entire history of work orders and assign tasks with ease.',
    author: 'Robert Williams',
    role: 'Facility Manager',
    company: 'Prime Real Estate Co.',
    rating: 5,
    metric: '50% faster maintenance response',
  },
]

export const TestimonialsSection: React.FC = () => {
  return (
    <section className="py-20 sm:py-32 bg-gradient-to-br from-blue-50 via-white to-purple-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Section Header */}
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-5xl font-bold text-gray-900 mb-4">
            Trusted by property managers everywhere
          </h2>
          <p className="text-xl text-gray-600 max-w-2xl mx-auto">
            See what our customers have to say about transforming their rental
            management business
          </p>
        </div>

        {/* Testimonials Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {testimonials.map((testimonial, index) => (
            <TestimonialCard key={index} {...testimonial} />
          ))}
        </div>
      </div>
    </section>
  )
}
