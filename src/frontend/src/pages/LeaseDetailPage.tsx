import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  leaseService,
  type LeaseDto,
  LeaseStatus,
  type LeaseStatusType,
} from '../services/leaseService'

const LeaseDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [lease, setLease] = useState<LeaseDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState(false)
  const [showTerminateModal, setShowTerminateModal] = useState(false)
  const [terminationReason, setTerminationReason] = useState('')

  useEffect(() => {
    if (id) {
      loadLease()
    }
  }, [id])

  const loadLease = async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const data = await leaseService.getLeaseById(id)
      setLease(data)
    } catch (err) {
      setError('Failed to load lease details')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleActivate = async () => {
    if (!id || !lease) return

    try {
      setActionLoading(true)
      const updated = await leaseService.activateLease(id)
      setLease(updated)
      alert('Lease activated successfully!')
    } catch (err) {
      alert('Failed to activate lease')
      console.error(err)
    } finally {
      setActionLoading(false)
    }
  }

  const handleTerminate = async () => {
    if (!id || !terminationReason.trim()) {
      alert('Please provide a termination reason')
      return
    }

    try {
      setActionLoading(true)
      const updated = await leaseService.terminateLease(id, terminationReason)
      setLease(updated)
      setShowTerminateModal(false)
      setTerminationReason('')
      alert('Lease terminated successfully')
    } catch (err) {
      alert('Failed to terminate lease')
      console.error(err)
    } finally {
      setActionLoading(false)
    }
  }

  const getStatusBadge = (status: LeaseStatusType) => {
    const colors: Record<number, string> = {
      [LeaseStatus.Draft]: 'bg-gray-100 text-gray-800',
      [LeaseStatus.Active]: 'bg-green-100 text-green-800',
      [LeaseStatus.Expired]: 'bg-yellow-100 text-yellow-800',
      [LeaseStatus.Terminated]: 'bg-red-100 text-red-800',
      [LeaseStatus.Renewed]: 'bg-blue-100 text-blue-800',
    }

    const statusText = Object.keys(LeaseStatus).find(
      key => LeaseStatus[key as keyof typeof LeaseStatus] === status
    )

    return (
      <span
        className={`px-3 py-1 text-sm font-semibold rounded-full ${colors[status]}`}
      >
        {statusText || status}
      </span>
    )
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading lease details...</p>
        </div>
      </div>
    )
  }

  if (error || !lease) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600">{error || 'Lease not found'}</p>
          <button
            onClick={() => navigate('/leases')}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Back to Leases
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <button
          onClick={() => navigate('/leases')}
          className="mb-6 text-blue-600 hover:text-blue-700 flex items-center"
        >
          ← Back to Leases
        </button>

        <div className="bg-white rounded-lg shadow-lg overflow-hidden">
          {/* Header */}
          <div className="bg-blue-600 px-6 py-8 text-white">
            <div className="flex justify-between items-start">
              <div>
                <h1 className="text-3xl font-bold">Lease Agreement</h1>
                <p className="mt-2 text-blue-100">ID: {lease.id}</p>
              </div>
              {getStatusBadge(lease.status)}
            </div>
          </div>

          {/* Content */}
          <div className="p-6 space-y-6">
            {/* Financial Information */}
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Financial Details
              </h2>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Monthly Rent</p>
                  <p className="text-lg font-semibold">
                    ${lease.monthlyRent.toFixed(2)} {lease.rentCurrency}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Security Deposit</p>
                  <p className="text-lg font-semibold">
                    ${lease.securityDeposit.toFixed(2)}{' '}
                    {lease.securityDepositCurrency}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Payment Day</p>
                  <p className="text-lg font-semibold">
                    Day {lease.paymentDayOfMonth} of month
                  </p>
                </div>
              </div>
            </div>

            {/* Lease Period */}
            <div className="border-t pt-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Lease Period
              </h2>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Start Date</p>
                  <p className="text-lg font-semibold">
                    {new Date(lease.startDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">End Date</p>
                  <p className="text-lg font-semibold">
                    {new Date(lease.endDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Duration</p>
                  <p className="text-lg font-semibold">
                    {lease.durationInDays} days
                  </p>
                </div>
                {lease.isActive && (
                  <div>
                    <p className="text-sm text-gray-600">Days Remaining</p>
                    <p className="text-lg font-semibold text-green-600">
                      {lease.remainingDays} days
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Special Terms */}
            {lease.specialTerms && (
              <div className="border-t pt-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-4">
                  Special Terms
                </h2>
                <p className="text-gray-700 whitespace-pre-wrap">
                  {lease.specialTerms}
                </p>
              </div>
            )}

            {/* Termination Info */}
            {lease.terminatedAt && (
              <div className="border-t pt-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-4">
                  Termination Details
                </h2>
                <div className="bg-red-50 p-4 rounded-md">
                  <p className="text-sm text-gray-600">Terminated On</p>
                  <p className="text-lg font-semibold">
                    {new Date(lease.terminatedAt).toLocaleDateString()}
                  </p>
                  {lease.terminationReason && (
                    <>
                      <p className="text-sm text-gray-600 mt-2">Reason</p>
                      <p className="text-gray-700">{lease.terminationReason}</p>
                    </>
                  )}
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="border-t pt-6 flex space-x-4">
              {lease.status === LeaseStatus.Draft && (
                <button
                  onClick={handleActivate}
                  disabled={actionLoading}
                  className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
                >
                  {actionLoading ? 'Activating...' : 'Activate Lease'}
                </button>
              )}

              {lease.status === LeaseStatus.Active && (
                <button
                  onClick={() => setShowTerminateModal(true)}
                  disabled={actionLoading}
                  className="px-6 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50"
                >
                  Terminate Lease
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Terminate Modal */}
      {showTerminateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-semibold mb-4">Terminate Lease</h3>
            <p className="text-gray-600 mb-4">
              Please provide a reason for terminating this lease.
            </p>
            <textarea
              value={terminationReason}
              onChange={e => setTerminationReason(e.target.value)}
              className="w-full border border-gray-300 rounded-md p-2 mb-4 min-h-[100px]"
              placeholder="Enter termination reason..."
            />
            <div className="flex space-x-4">
              <button
                onClick={handleTerminate}
                disabled={actionLoading || !terminationReason.trim()}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50"
              >
                {actionLoading ? 'Terminating...' : 'Confirm Termination'}
              </button>
              <button
                onClick={() => {
                  setShowTerminateModal(false)
                  setTerminationReason('')
                }}
                disabled={actionLoading}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default LeaseDetailPage
