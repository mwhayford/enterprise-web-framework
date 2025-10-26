// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '../ui/Button'

interface NavigationProps {
  onGetStarted: () => void
}

export const Navigation: React.FC<NavigationProps> = ({ onGetStarted }) => {
  const navigate = useNavigate()
  const [isScrolled, setIsScrolled] = useState(false)
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20)
    }

    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const scrollToSection = (sectionId: string) => {
    const element = document.getElementById(sectionId)
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' })
      setIsMobileMenuOpen(false)
    }
  }

  const handleKeyDown = (event: React.KeyboardEvent, sectionId: string) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      scrollToSection(sectionId)
    }
  }

  return (
    <nav
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-200 ${
        isScrolled
          ? 'bg-white/95 backdrop-blur-md shadow-md'
          : 'bg-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-20">
          {/* Logo */}
          <div className="flex items-center">
            <button
              onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
              className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent hover:from-blue-700 hover:to-purple-700 transition-all"
              type="button"
            >
              RentalManager
            </button>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center gap-8">
            <button
              onClick={() => navigate('/properties')}
              className="text-gray-700 hover:text-blue-600 transition-colors font-medium"
              type="button"
            >
              Properties
            </button>
            <button
              onClick={() => scrollToSection('features')}
              onKeyDown={(e) => handleKeyDown(e, 'features')}
              className="text-gray-700 hover:text-blue-600 transition-colors font-medium"
              type="button"
            >
              Features
            </button>
            <button
              onClick={() => scrollToSection('pricing')}
              onKeyDown={(e) => handleKeyDown(e, 'pricing')}
              className="text-gray-700 hover:text-blue-600 transition-colors font-medium"
              type="button"
            >
              Pricing
            </button>
            <button
              onClick={() => scrollToSection('testimonials')}
              onKeyDown={(e) => handleKeyDown(e, 'testimonials')}
              className="text-gray-700 hover:text-blue-600 transition-colors font-medium"
              type="button"
            >
              Testimonials
            </button>
            <button
              onClick={() => scrollToSection('faq')}
              onKeyDown={(e) => handleKeyDown(e, 'faq')}
              className="text-gray-700 hover:text-blue-600 transition-colors font-medium"
              type="button"
            >
              FAQ
            </button>
          </div>

          {/* CTA Buttons */}
          <div className="hidden md:flex items-center gap-4">
            <Button variant="outline" onClick={() => navigate('/login')}>
              Sign In
            </Button>
            <Button onClick={onGetStarted}>Get Started</Button>
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            aria-label="Toggle menu"
            aria-expanded={isMobileMenuOpen ? 'true' : 'false'}
            type="button"
          >
            <svg
              className="w-6 h-6 text-gray-700"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              {isMobileMenuOpen ? (
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              ) : (
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 6h16M4 12h16M4 18h16"
                />
              )}
            </svg>
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden bg-white border-t border-gray-200 shadow-lg">
          <div className="px-4 py-6 space-y-4">
            <button
              onClick={() => {
                navigate('/properties')
                setIsMobileMenuOpen(false)
              }}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium"
              type="button"
            >
              Properties
            </button>
            <button
              onClick={() => scrollToSection('features')}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium"
              type="button"
            >
              Features
            </button>
            <button
              onClick={() => scrollToSection('pricing')}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium"
              type="button"
            >
              Pricing
            </button>
            <button
              onClick={() => scrollToSection('testimonials')}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium"
              type="button"
            >
              Testimonials
            </button>
            <button
              onClick={() => scrollToSection('faq')}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium"
              type="button"
            >
              FAQ
            </button>
            <div className="pt-4 border-t border-gray-200 space-y-2">
              <Button
                variant="outline"
                onClick={() => {
                  navigate('/login')
                  setIsMobileMenuOpen(false)
                }}
                className="w-full"
              >
                Sign In
              </Button>
              <Button
                onClick={() => {
                  onGetStarted()
                  setIsMobileMenuOpen(false)
                }}
                className="w-full"
              >
                Get Started
              </Button>
            </div>
          </div>
        </div>
      )}
    </nav>
  )
}

