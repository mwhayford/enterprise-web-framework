// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState, useEffect } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { Menu, X } from 'lucide-react'
import { Button } from '../ui/Button'
import { cn } from '../../utils/cn'

export const PublicNavigation: React.FC = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const [isScrolled, setIsScrolled] = useState(false)
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20)
    }

    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const navigationItems = [
    { path: '/', label: 'Home' },
    { path: '/properties', label: 'Browse Properties' },
    { path: '/how-it-works', label: 'How It Works' },
    { path: '/#pricing', label: 'Pricing' },
  ]

  const isActive = (path: string) => {
    if (path === '/') {
      return location.pathname === '/'
    }
    if (path.startsWith('/#')) {
      return location.pathname === '/'
    }
    return location.pathname === path || location.pathname.startsWith(path)
  }

  const scrollToSection = (sectionId: string) => {
    if (location.pathname !== '/') {
      navigate('/')
      setTimeout(() => {
        const element = document.getElementById(sectionId.replace('#', ''))
        element?.scrollIntoView({ behavior: 'smooth' })
      }, 100)
    } else {
      const element = document.getElementById(sectionId.replace('#', ''))
      element?.scrollIntoView({ behavior: 'smooth' })
    }
    setIsMobileMenuOpen(false)
  }

  const handleNavClick = (path: string) => {
    if (path.startsWith('/#')) {
      scrollToSection(path)
    } else {
      navigate(path)
      setIsMobileMenuOpen(false)
    }
  }

  return (
    <nav
      className={cn(
        'fixed top-0 left-0 right-0 z-50 transition-all duration-200',
        isScrolled ? 'bg-white/95 backdrop-blur-md shadow-md' : 'bg-transparent'
      )}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-20">
          {/* Logo */}
          <Link
            to="/"
            className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent hover:from-blue-700 hover:to-purple-700 transition-all"
          >
            RentalManager
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center gap-8">
            {navigationItems.map(item => (
              <button
                key={item.path}
                onClick={() => handleNavClick(item.path)}
                className={cn(
                  'text-gray-700 hover:text-blue-600 transition-colors font-medium',
                  isActive(item.path) && 'text-blue-600'
                )}
                type="button"
              >
                {item.label}
              </button>
            ))}
          </div>

          {/* Right Side - Actions */}
          <div className="hidden md:flex items-center gap-4">
            <Button variant="outline" onClick={() => navigate('/login')}>
              Sign In
            </Button>
            <Button onClick={() => navigate('/login')}>Get Started</Button>
          </div>

          {/* Mobile Menu Button */}
          <button
            type="button"
            className="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            aria-label="Toggle menu"
            aria-expanded={isMobileMenuOpen}
          >
            {isMobileMenuOpen ? (
              <X className="w-6 h-6 text-gray-700" />
            ) : (
              <Menu className="w-6 h-6 text-gray-700" />
            )}
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden bg-white border-t border-gray-200 shadow-lg">
          <div className="px-4 py-6 space-y-4">
            {navigationItems.map(item => (
              <button
                key={item.path}
                onClick={() => handleNavClick(item.path)}
                className={cn(
                  'block w-full text-left px-4 py-2 text-gray-700 hover:bg-blue-50 hover:text-blue-600 rounded-lg transition-colors font-medium',
                  isActive(item.path) && 'bg-blue-50 text-blue-600'
                )}
                type="button"
              >
                {item.label}
              </button>
            ))}
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
                  navigate('/login')
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
