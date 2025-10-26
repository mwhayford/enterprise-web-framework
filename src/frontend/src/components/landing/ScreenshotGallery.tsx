// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState } from 'react'

interface Screenshot {
  title: string
  description: string
  imageUrl: string
  category: string
}

const screenshots: Screenshot[] = [
  {
    title: "Dashboard Overview",
    description: "Get a complete view of your rental business at a glance",
    imageUrl: "/screenshots/dashboard.jpg",
    category: "Dashboard",
  },
  {
    title: "Property Management",
    description: "Manage all your properties with detailed profiles and documents",
    imageUrl: "/screenshots/properties.jpg",
    category: "Properties",
  },
  {
    title: "Payment Processing",
    description: "Accept rent payments online with automatic reminders",
    imageUrl: "/screenshots/payments.jpg",
    category: "Payments",
  },
  {
    title: "Analytics & Reports",
    description: "Track your revenue and performance with real-time analytics",
    imageUrl: "/screenshots/analytics.jpg",
    category: "Analytics",
  },
]

export const ScreenshotGallery: React.FC = () => {
  const [selectedImage, setSelectedImage] = useState<Screenshot | null>(null)

  const handleImageClick = (screenshot: Screenshot) => {
    setSelectedImage(screenshot)
  }

  const handleCloseLightbox = () => {
    setSelectedImage(null)
  }

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      handleCloseLightbox()
    }
  }

  return (
    <section className="py-20 sm:py-32 bg-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Section Header */}
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-5xl font-bold text-gray-900 mb-4">
            See RentalManager in action
          </h2>
          <p className="text-xl text-gray-600 max-w-2xl mx-auto">
            Explore our intuitive interface designed for property managers
          </p>
        </div>

        {/* Screenshot Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {screenshots.map((screenshot, index) => (
            <div
              key={index}
              className="group cursor-pointer"
              onClick={() => handleImageClick(screenshot)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault()
                  handleImageClick(screenshot)
                }
              }}
              role="button"
              tabIndex={0}
              aria-label={`View ${screenshot.title} screenshot`}
            >
              <div className="relative overflow-hidden rounded-xl shadow-lg hover:shadow-2xl transition-shadow">
                {/* Placeholder with gradient */}
                <div className="aspect-video bg-gradient-to-br from-blue-100 via-purple-100 to-pink-100 flex items-center justify-center">
                  <div className="text-center p-8">
                    <div className="w-16 h-16 mx-auto mb-4 bg-white rounded-lg flex items-center justify-center">
                      <svg
                        className="w-8 h-8 text-blue-600"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                        />
                      </svg>
                    </div>
                    <p className="text-sm text-gray-600 font-medium">
                      {screenshot.category}
                    </p>
                  </div>
                </div>
                {/* Overlay on hover */}
                <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/0 to-black/0 opacity-0 group-hover:opacity-100 transition-opacity flex items-end p-6">
                  <div className="text-white">
                    <p className="text-sm font-semibold mb-1">Click to enlarge</p>
                  </div>
                </div>
              </div>
              <div className="mt-4">
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  {screenshot.title}
                </h3>
                <p className="text-gray-600">{screenshot.description}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Lightbox Modal */}
      {selectedImage && (
        <div
          className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center p-4"
          onClick={handleCloseLightbox}
          onKeyDown={handleKeyDown}
          role="dialog"
          aria-modal="true"
          aria-label="Screenshot preview"
        >
          <button
            className="absolute top-4 right-4 text-white hover:text-gray-300 transition-colors"
            onClick={handleCloseLightbox}
            aria-label="Close preview"
            type="button"
          >
            <svg
              className="w-8 h-8"
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
          </button>
          <div className="max-w-6xl w-full" onClick={(e) => e.stopPropagation()}>
            <div className="bg-gradient-to-br from-blue-100 via-purple-100 to-pink-100 rounded-lg aspect-video flex items-center justify-center">
              <div className="text-center p-12">
                <div className="w-24 h-24 mx-auto mb-6 bg-white rounded-lg flex items-center justify-center">
                  <svg
                    className="w-12 h-12 text-blue-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                </div>
                <h3 className="text-2xl font-bold text-gray-900 mb-2">
                  {selectedImage.title}
                </h3>
                <p className="text-gray-600">{selectedImage.description}</p>
              </div>
            </div>
            <div className="mt-4 text-center">
              <p className="text-white text-sm">
                Press ESC or click outside to close
              </p>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}

