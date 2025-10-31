import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  CheckCircle,
  XCircle,
  User,
  Briefcase,
  Home,
  Phone,
} from 'lucide-react'
import {
  applicationService,
  type PropertyApplicationDto,
} from '../services/applicationService'

const statusLabels: Record<number, string> = {
  0: 'Pending',
  1: 'Under Review',
  2: 'Approved',
  3: 'Rejected',
  4: 'Withdrawn',
}

const statusColors: Record<number, string> = {
  0: 'bg-yellow-100 text-yellow-800',
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-gray-100 text-gray-800',
}

export const ApplicationReviewPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [application, setApplication] = useState<PropertyApplicationDto | null>(
    null
  )
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [decisionNotes, setDecisionNotes] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (id) {
      loadApplication()
    }
  }, [id])

  const loadApplication = async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const data = await applicationService.getApplicationById(id)
      setApplication(data)
      if (data.decisionNotes) {
        setDecisionNotes(data.decisionNotes)
      }
    } catch (err) {
      setError('Failed to load application details.')
      console.error('Error loading application:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleApprove = async () => {
    if (!id || !application) return

    try {
      setSubmitting(true)
      setError(null)
      await applicationService.approveApplication(id, decisionNotes)
      navigate('/admin/applications', {
        state: { message: 'Application approved successfully!' },
      })
    } catch (err) {
      setError('Failed to approve application.')
      console.error('Error approving application:', err)
    } finally {
      setSubmitting(false)
    }
  }

  const handleReject = async () => {
    if (!id || !application) return

    try {
      setSubmitting(true)
      setError(null)
      await applicationService.rejectApplication(id, decisionNotes)
      navigate('/admin/applications', {
        state: { message: 'Application rejected.' },
      })
    } catch (err) {
      setError('Failed to reject application.')
      console.error('Error rejecting application:', err)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-600">Loading application...</div>
      </div>
    )
  }

  if (error && !application) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 text-lg mb-4">{error}</p>
          <button
            onClick={() => navigate('/admin/applications')}
            className="text-blue-600 hover:underline"
          >
            Back to Applications
          </button>
        </div>
      </div>
    )
  }

  if (!application) return null

  const applicationData = application.applicationData
    ? JSON.parse(application.applicationData)
    : {}
  const canReview = application.status === 0 || application.status === 1

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <button
            onClick={() => navigate('/admin/applications')}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
          >
            <ArrowLeft className="w-5 h-5" />
            <span>Back to Applications</span>
          </button>
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">
              Application Review
            </h1>
            <span
              className={`px-3 py-1 rounded-full text-sm font-semibold ${statusColors[application.status]}`}
            >
              {statusLabels[application.status]}
            </span>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Application Details */}
          <div className="lg:col-span-2 space-y-6">
            {/* Personal Information */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center gap-2 mb-4">
                <User className="w-5 h-5 text-gray-600" />
                <h2 className="text-xl font-bold">Personal Information</h2>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Name</p>
                  <p className="font-medium">
                    {applicationData.firstName} {applicationData.lastName}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Email</p>
                  <p className="font-medium">{applicationData.email}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Phone</p>
                  <p className="font-medium">{applicationData.phone}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Date of Birth</p>
                  <p className="font-medium">{applicationData.dateOfBirth}</p>
                </div>
              </div>
            </div>

            {/* Employment Information */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center gap-2 mb-4">
                <Briefcase className="w-5 h-5 text-gray-600" />
                <h2 className="text-xl font-bold">Employment Information</h2>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-600">Employer</p>
                  <p className="font-medium">{applicationData.employerName}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Job Title</p>
                  <p className="font-medium">{applicationData.jobTitle}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Annual Income</p>
                  <p className="font-medium">
                    ${applicationData.annualIncome?.toLocaleString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Years Employed</p>
                  <p className="font-medium">
                    {applicationData.yearsEmployed} years
                  </p>
                </div>
              </div>
            </div>

            {/* Rental History */}
            {applicationData.previousAddresses &&
              applicationData.previousAddresses.length > 0 && (
                <div className="bg-white rounded-lg shadow-md p-6">
                  <div className="flex items-center gap-2 mb-4">
                    <Home className="w-5 h-5 text-gray-600" />
                    <h2 className="text-xl font-bold">Previous Addresses</h2>
                  </div>
                  <div className="space-y-4">
                    {applicationData.previousAddresses.map(
                      (addr: Record<string, unknown>, index: number) => (
                        <div
                          key={index}
                          className="border-l-4 border-blue-500 pl-4"
                        >
                          <p className="font-medium">
                            {addr.address as string}
                          </p>
                          <p className="text-sm text-gray-600">
                            {addr.moveInDate as string} -{' '}
                            {addr.moveOutDate as string}
                          </p>
                          <p className="text-sm text-gray-600">
                            Landlord: {addr.landlordName as string} (
                            {addr.landlordPhone as string})
                          </p>
                          <p className="text-sm text-gray-600">
                            Monthly Rent: $
                            {(addr.monthlyRent as number).toLocaleString()}
                          </p>
                          <p className="text-sm text-gray-600">
                            Reason for Leaving:{' '}
                            {addr.reasonForLeaving as string}
                          </p>
                        </div>
                      )
                    )}
                  </div>
                </div>
              )}

            {/* References */}
            {applicationData.references &&
              applicationData.references.length > 0 && (
                <div className="bg-white rounded-lg shadow-md p-6">
                  <div className="flex items-center gap-2 mb-4">
                    <Phone className="w-5 h-5 text-gray-600" />
                    <h2 className="text-xl font-bold">References</h2>
                  </div>
                  <div className="space-y-3">
                    {applicationData.references.map(
                      (ref: Record<string, unknown>, index: number) => (
                        <div
                          key={index}
                          className="border-b pb-3 last:border-b-0"
                        >
                          <p className="font-medium">{ref.name as string}</p>
                          <p className="text-sm text-gray-600">
                            {ref.relationship as string}
                          </p>
                          <p className="text-sm text-gray-600">
                            {ref.phone as string} • {ref.email as string}
                          </p>
                        </div>
                      )
                    )}
                  </div>
                </div>
              )}

            {/* Additional Notes */}
            {applicationData.additionalNotes && (
              <div className="bg-white rounded-lg shadow-md p-6">
                <h2 className="text-xl font-bold mb-4">Additional Notes</h2>
                <p className="text-gray-700 whitespace-pre-line">
                  {applicationData.additionalNotes}
                </p>
              </div>
            )}
          </div>

          {/* Review Panel */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-md p-6 sticky top-4">
              <h2 className="text-xl font-bold mb-4">Review Decision</h2>

              <div className="space-y-4 mb-6">
                <div>
                  <p className="text-sm text-gray-600">Application Fee</p>
                  <p className="text-lg font-semibold">
                    {application.applicationFeeCurrency}{' '}
                    {application.applicationFee.toLocaleString()}
                  </p>
                  <p className="text-sm text-gray-600">
                    {application.applicationFeePaymentId
                      ? 'Paid'
                      : 'Payment Pending'}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-600">Submitted</p>
                  <p className="font-medium">
                    {application.submittedAt
                      ? new Date(application.submittedAt).toLocaleDateString(
                          'en-US',
                          {
                            month: 'long',
                            day: 'numeric',
                            year: 'numeric',
                          }
                        )
                      : 'N/A'}
                  </p>
                </div>

                {application.reviewedAt && (
                  <div>
                    <p className="text-sm text-gray-600">Reviewed</p>
                    <p className="font-medium">
                      {new Date(application.reviewedAt).toLocaleDateString(
                        'en-US',
                        {
                          month: 'long',
                          day: 'numeric',
                          year: 'numeric',
                        }
                      )}
                    </p>
                  </div>
                )}
              </div>

              <div className="mb-6">
                <label
                  htmlFor="decisionNotes"
                  className="block text-sm font-medium text-gray-700 mb-2"
                >
                  Decision Notes
                </label>
                <textarea
                  id="decisionNotes"
                  value={decisionNotes}
                  onChange={e => setDecisionNotes(e.target.value)}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Add notes about your decision..."
                  disabled={!canReview}
                />
              </div>

              {error && (
                <div className="mb-4 bg-red-50 border border-red-200 rounded-lg p-3 text-red-800 text-sm">
                  {error}
                </div>
              )}

              {canReview ? (
                <div className="space-y-2">
                  <button
                    onClick={handleApprove}
                    disabled={submitting}
                    className="w-full flex items-center justify-center gap-2 bg-green-600 text-white py-3 px-4 rounded-md hover:bg-green-700 transition-colors font-semibold disabled:opacity-50"
                  >
                    <CheckCircle className="w-5 h-5" />
                    <span>
                      {submitting ? 'Approving...' : 'Approve Application'}
                    </span>
                  </button>
                  <button
                    onClick={handleReject}
                    disabled={submitting}
                    className="w-full flex items-center justify-center gap-2 bg-red-600 text-white py-3 px-4 rounded-md hover:bg-red-700 transition-colors font-semibold disabled:opacity-50"
                  >
                    <XCircle className="w-5 h-5" />
                    <span>
                      {submitting ? 'Rejecting...' : 'Reject Application'}
                    </span>
                  </button>
                </div>
              ) : (
                <div className="bg-gray-100 text-gray-600 py-3 px-4 rounded-md text-center">
                  This application has been reviewed
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
