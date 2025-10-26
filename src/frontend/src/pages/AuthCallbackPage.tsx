// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../components/ui/Card'

const AuthCallbackPage: React.FC = () => {
  const navigate = useNavigate()

  useEffect(() => {
    const handleAuthCallback = async () => {
      try {
        // Get the URL parameters
        const urlParams = new URLSearchParams(window.location.search)
        const token = urlParams.get('token')
        const user = urlParams.get('user')

        if (token && user) {
          // Parse user data
          const userData = JSON.parse(decodeURIComponent(user))

          // Store tokens and user data in localStorage (using correct keys)
          localStorage.setItem('auth_token', token)
          localStorage.setItem('user', JSON.stringify(userData))

          // Redirect to dashboard (this will trigger AuthContext to reload user)
          navigate('/dashboard', { replace: true })
        } else {
          // Handle error case
          console.error('Missing token or user data in callback')
          navigate('/login?error=auth_failed', { replace: true })
        }
      } catch (error) {
        console.error('Error processing auth callback:', error)
        navigate('/login?error=auth_failed', { replace: true })
      }
    }

    handleAuthCallback()
  }, [navigate])

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <Card>
          <CardHeader>
            <CardTitle>Completing Sign In</CardTitle>
            <CardDescription>
              Please wait while we complete your authentication...
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default AuthCallbackPage
