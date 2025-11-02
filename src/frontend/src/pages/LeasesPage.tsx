import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  leaseService,
  type LeaseDto,
  LeaseStatus,
  type LeaseStatusType,
} from '../services/leaseService'
import { AuthenticatedLayout } from '../components/layout/AuthenticatedLayout'

const LeaseStatusBadge = ({ status }: { status: LeaseStatusType }) => {
  const getStatusColor = () => {
    switch (status) {
      case LeaseStatus.Active:
        return 'bg-green-100 text-green-800'
      case LeaseStatus.Draft:
        return 'bg-gray-100 text-gray-800'
      case LeaseStatus.Expired:
        return 'bg-yellow-100 text-yellow-800'
      case LeaseStatus.Terminated:
        return 'bg-red-100 text-red-800'
      case LeaseStatus.Renewed:
        return 'bg-blue-100 text-blue-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusText = () => {
    return (
      Object.keys(LeaseStatus).find(
        key => LeaseStatus[key as keyof typeof LeaseStatus] === status
      ) || status
    )
  }

  return (
    <span
      className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor()}`}
    >
      {getStatusText()}
    </span>
  )
}

const LeasesPage = () => {
  const [leases, setLeases] = useState<LeaseDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [viewMode, setViewMode] = useState<'tenant' | 'landlord'>('tenant')
  const navigate = useNavigate()

  // Get current user ID from localStorage (you might have this in a context)
  const currentUserId = localStorage.getItem('user_id') || ''

  const loadLeases = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)

      const data =
        viewMode === 'tenant'
          ? await leaseService.getLeasesByTenantId(currentUserId)
          : await leaseService.getLeasesByTenantId(currentUserId) // TODO: Add landlord endpoint

      setLeases(data)
    } catch (err) {
      setError('Failed to load leases')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [viewMode, currentUserId])

  useEffect(() => {
    loadLeases()
  }, [loadLeases])

  const handleViewLease = (leaseId: string) => {
    navigate(`/leases/${leaseId}`)
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading leases...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600">{error}</p>
          <button
            onClick={loadLeases}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Retry
          </button>
        </div>
      </div>
    )
  }

  return (
    <AuthenticatedLayout>
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">My Leases</h1>
        <p className="mt-2 text-gray-600">View and manage your rental leases</p>
      </div>

      {/* View Mode Toggle */}
      <div className="mb-6 flex space-x-2">
        <button
          onClick={() => setViewMode('tenant')}
          className={`px-4 py-2 rounded-md ${
            viewMode === 'tenant'
              ? 'bg-blue-600 text-white'
              : 'bg-white text-gray-700 hover:bg-gray-50'
          }`}
        >
          As Tenant
        </button>
        <button
          onClick={() => setViewMode('landlord')}
          className={`px-4 py-2 rounded-md ${
            viewMode === 'landlord'
              ? 'bg-blue-600 text-white'
              : 'bg-white text-gray-700 hover:bg-gray-50'
          }`}
        >
          As Landlord
        </button>
      </div>

      {leases.length === 0 ? (
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <p className="text-gray-600">No leases found</p>
          <button
            onClick={() => navigate('/properties')}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Browse Properties
          </button>
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {leases.map(lease => (
            <div
              key={lease.id}
              className="bg-white rounded-lg shadow hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => handleViewLease(lease.id)}
            >
              <div className="p-6">
                <div className="flex justify-between items-start mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">
                    Property {lease.propertyId.substring(0, 8)}...
                  </h3>
                  <LeaseStatusBadge status={lease.status} />
                </div>

                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Monthly Rent:</span>
                    <span className="font-semibold">
                      ${lease.monthlyRent.toFixed(2)} {lease.rentCurrency}
                    </span>
                  </div>

                  <div className="flex justify-between">
                    <span className="text-gray-600">Start Date:</span>
                    <span>
                      {new Date(lease.startDate).toLocaleDateString()}
                    </span>
                  </div>

                  <div className="flex justify-between">
                    <span className="text-gray-600">End Date:</span>
                    <span>{new Date(lease.endDate).toLocaleDateString()}</span>
                  </div>

                  {lease.isActive && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Days Remaining:</span>
                      <span className="font-semibold text-green-600">
                        {lease.remainingDays}
                      </span>
                    </div>
                  )}
                </div>

                <div className="mt-4 pt-4 border-t border-gray-200">
                  <button
                    onClick={e => {
                      e.stopPropagation()
                      handleViewLease(lease.id)
                    }}
                    className="w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                  >
                    View Details
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </AuthenticatedLayout>
  )
}

export default LeasesPage
