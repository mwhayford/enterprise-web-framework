// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react'
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from 'react-router-dom'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import LandingPage from './pages/LandingPage'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import AuthCallbackPage from './pages/AuthCallbackPage'
import { PaymentPage } from './pages/PaymentPage'
import { SubscriptionPage } from './pages/SubscriptionPage'
import { PaymentMethodsPage } from './pages/PaymentMethodsPage'
import { PaymentSuccessPage } from './pages/PaymentSuccessPage'
import { SubscriptionSuccessPage } from './pages/SubscriptionSuccessPage'
import SearchPage from './pages/SearchPage'
import { PropertiesPage } from './pages/PropertiesPage'
import { PropertyDetailPage } from './pages/PropertyDetailPage'
import { ApplicationFormPage } from './pages/ApplicationFormPage'
import { MyApplicationsPage } from './pages/MyApplicationsPage'
import { AdminApplicationsPage } from './pages/AdminApplicationsPage'
import { ApplicationReviewPage } from './pages/ApplicationReviewPage'
import LeasesPage from './pages/LeasesPage'
import LeaseDetailPage from './pages/LeaseDetailPage'

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

const PublicRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return !isAuthenticated ? (
    <>{children}</>
  ) : (
    <Navigate to="/dashboard" replace />
  )
}

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginPage />
          </PublicRoute>
        }
      />
      <Route path="/auth-callback" element={<AuthCallbackPage />} />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/payment"
        element={
          <ProtectedRoute>
            <PaymentPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscription"
        element={
          <ProtectedRoute>
            <SubscriptionPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/payment-methods"
        element={
          <ProtectedRoute>
            <PaymentMethodsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/payment-success"
        element={
          <ProtectedRoute>
            <PaymentSuccessPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscription-success"
        element={
          <ProtectedRoute>
            <SubscriptionSuccessPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/search"
        element={
          <ProtectedRoute>
            <SearchPage />
          </ProtectedRoute>
        }
      />
      <Route path="/properties" element={<PropertiesPage />} />
      <Route path="/properties/:id" element={<PropertyDetailPage />} />
      <Route
        path="/properties/:id/apply"
        element={
          <ProtectedRoute>
            <ApplicationFormPage />
          </ProtectedRoute>
        }
      />
                <Route
                  path="/applications/my"
                  element={
                    <ProtectedRoute>
                      <MyApplicationsPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/admin/applications"
                  element={
                    <ProtectedRoute>
                      <AdminApplicationsPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/admin/applications/:id"
                  element={
                    <ProtectedRoute>
                      <ApplicationReviewPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/leases"
                  element={
                    <ProtectedRoute>
                      <LeasesPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/leases/:id"
                  element={
                    <ProtectedRoute>
                      <LeaseDetailPage />
                    </ProtectedRoute>
                  }
                />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <AppRoutes />
        </div>
      </Router>
    </AuthProvider>
  )
}

export default App
