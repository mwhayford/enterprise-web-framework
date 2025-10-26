// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import { FAQItem } from './FAQItem'

const faqs = [
  {
    question: "How much does RentalManager cost?",
    answer:
      "We offer three pricing tiers to fit your needs: Starter at $29/month for up to 10 properties, Professional at $79/month for up to 50 properties, and Enterprise with custom pricing for larger portfolios. All plans include a 14-day free trial with no credit card required.",
  },
  {
    question: "How long does it take to set up?",
    answer:
      "Most users are up and running in under an hour. Simply create your account, add your properties, invite tenants, and you're ready to go. We provide guided onboarding and our support team is available to help you get started quickly.",
  },
  {
    question: "Is my data secure?",
    answer:
      "Absolutely. We use bank-level encryption (256-bit SSL) to protect your data. All payment information is processed through Stripe, a PCI-compliant payment processor. We perform regular security audits and backups to ensure your data is always safe and accessible.",
  },
  {
    question: "Can I import my existing data?",
    answer:
      "Yes! We provide easy-to-use import tools for properties, tenants, and lease information. Our support team can also assist with data migration from other property management systems. We support CSV imports and offer API access for custom integrations.",
  },
  {
    question: "What payment methods do you accept?",
    answer:
      "We accept all major credit cards, debit cards, and ACH bank transfers through our integration with Stripe. Tenants can save their payment methods for easy recurring payments, and you can set up automatic payment reminders.",
  },
  {
    question: "Do you offer customer support?",
    answer:
      "Yes! We provide email support for all plans, with priority support for Professional and Enterprise customers. Our support team typically responds within 24 hours. We also have extensive documentation, video tutorials, and a knowledge base to help you find answers quickly.",
  },
  {
    question: "Can I cancel anytime?",
    answer:
      "Yes, there are no long-term contracts. You can cancel your subscription at any time from your account settings. If you cancel, you'll continue to have access until the end of your billing period. You can also export your data at any time.",
  },
  {
    question: "What integrations are available?",
    answer:
      "RentalManager integrates with Stripe for payments, Google OAuth for authentication, and provides a REST API for custom integrations. We're constantly adding new integrations based on customer feedback. Enterprise customers can work with our team for custom integration needs.",
  },
  {
    question: "Is there a mobile app?",
    answer:
      "Our web application is fully responsive and works great on mobile devices. A dedicated mobile app is on our roadmap for 2025. In the meantime, you can access all features through your mobile browser with a seamless experience.",
  },
  {
    question: "How many properties can I manage?",
    answer:
      "It depends on your plan: Starter supports up to 10 properties, Professional up to 50 properties, and Enterprise has no limits. You can easily upgrade your plan as your portfolio grows, and we'll migrate your data seamlessly.",
  },
]

export const FAQSection: React.FC = () => {
  return (
    <section id="faq" className="py-20 sm:py-32 bg-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Section Header */}
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-5xl font-bold text-gray-900 mb-4">
            Frequently Asked Questions
          </h2>
          <p className="text-xl text-gray-600 max-w-2xl mx-auto">
            Everything you need to know about RentalManager
          </p>
        </div>

        {/* FAQ Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-x-12">
          <div className="bg-white rounded-xl">
            {faqs.slice(0, 5).map((faq, index) => (
              <FAQItem key={index} question={faq.question} answer={faq.answer} />
            ))}
          </div>
          <div className="bg-white rounded-xl">
            {faqs.slice(5).map((faq, index) => (
              <FAQItem key={index + 5} question={faq.question} answer={faq.answer} />
            ))}
          </div>
        </div>

        {/* Contact CTA */}
        <div className="mt-12 text-center">
          <p className="text-gray-600 text-lg">
            Still have questions?{' '}
            <a
              href="mailto:support@rentalmanager.com"
              className="text-blue-600 hover:text-blue-700 font-semibold"
            >
              Contact our support team
            </a>
          </p>
        </div>
      </div>
    </section>
  )
}

