import { useState, useEffect } from 'react'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import {
  FileText,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
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

const statusIcons: Record<number, React.ReactNode> = {
  0: <Clock className="w-5 h-5 text-yellow-600" />,
  1: <AlertCircle className="w-5 h-5 text-blue-600" />,
  2: <CheckCircle className="w-5 h-5 text-green-600" />,
  3: <XCircle className="w-5 h-5 text-red-600" />,
  4: <XCircle className="w-5 h-5 text-gray-600" />,
}

const statusColors: Record<number, string> = {
  0: 'bg-yellow-100 text-yellow-800',
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-gray-100 text-gray-800',
}

export const MyApplicationsPage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const [applications, setApplications] = useState<PropertyApplicationDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  useEffect(() => {
    loadApplications()

    // Show success message if redirected after submission
    if (location.state && location.state.message) {
      setSuccessMessage(location.state.message)
      // Clear the message from location state
      navigate(location.pathname, { replace: true })
    }
  }, [])

  const loadApplications = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await applicationService.getMyApplications()
      setApplications(data)
    } catch (err) {
      setError('Failed to load your applications. Please try again later.')
      console.error('Error loading applications:', err)
    } finally {
      setLoading(false)
    }
  }

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    })
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-3xl font-bold text-gray-900">My Applications</h1>
          <p className="mt-2 text-gray-600">
            Track the status of your rental applications
          </p>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {successMessage && (
          <div className="mb-6 bg-green-50 border border-green-200 rounded-lg p-4 text-green-800 flex items-center gap-2">
            <CheckCircle className="w-5 h-5" />
            <span>{successMessage}</span>
          </div>
        )}

        {loading && (
          <div className="flex items-center justify-center h-64">
            <div className="text-gray-600">Loading your applications...</div>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
            {error}
          </div>
        )}

        {!loading && !error && applications.length === 0 && (
          <div className="bg-white rounded-lg shadow-md p-12 text-center">
            <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-900 mb-2">
              No Applications Yet
            </h2>
            <p className="text-gray-600 mb-6">
              You haven't submitted any rental applications yet. Browse
              available properties to get started!
            </p>
            <Link
              to="/properties"
              className="inline-block bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 transition-colors"
            >
              Browse Properties
            </Link>
          </div>
        )}

        {!loading && !error && applications.length > 0 && (
          <div className="space-y-4">
            {applications.map(application => {
              const applicationData = application.applicationData
                ? JSON.parse(application.applicationData)
                : null

              return (
                <div
                  key={application.id}
                  className="bg-white rounded-lg shadow-md overflow-hidden"
                >
                  <div className="p-6">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                          <h3 className="text-lg font-semibold text-gray-900">
                            Property ID:{' '}
                            {application.propertyId.substring(0, 8)}...
                          </h3>
                          <div
                            className={`flex items-center gap-2 px-3 py-1 rounded-full text-sm font-semibold ${statusColors[application.status]}`}
                          >
                            {statusIcons[application.status]}
                            <span>{statusLabels[application.status]}</span>
                          </div>
                        </div>

                        {applicationData && (
                          <p className="text-gray-600 text-sm mb-3">
                            Applicant: {applicationData.firstName}{' '}
                            {applicationData.lastName}
                          </p>
                        )}

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                          <div>
                            <span className="text-gray-600">Submitted:</span>
                            <span className="ml-2 font-medium">
                              {formatDate(application.submittedAt)}
                            </span>
                          </div>
                          <div>
                            <span className="text-gray-600">
                              Application Fee:
                            </span>
                            <span className="ml-2 font-medium">
                              {application.applicationFeeCurrency}{' '}
                              {application.applicationFee.toLocaleString()}
                            </span>
                          </div>
                          {application.reviewedAt && (
                            <div>
                              <span className="text-gray-600">Reviewed:</span>
                              <span className="ml-2 font-medium">
                                {formatDate(application.reviewedAt)}
                              </span>
                            </div>
                          )}
                        </div>

                        {application.decisionNotes && (
                          <div className="mt-4 p-3 bg-gray-50 rounded-md">
                            <p className="text-sm font-medium text-gray-700 mb-1">
                              Decision Notes:
                            </p>
                            <p className="text-sm text-gray-600">
                              {application.decisionNotes}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>

                    <div className="mt-4 pt-4 border-t border-gray-200 flex items-center justify-between">
                      <Link
                        to={`/properties/${application.propertyId}`}
                        className="text-blue-600 hover:text-blue-700 text-sm font-medium"
                      >
                        View Property Details →
                      </Link>

                      {application.status === 0 &&
                        !application.applicationFeePaymentId && (
                          <div className="flex items-center gap-2">
                            <span className="text-sm text-gray-600">
                              Payment pending
                            </span>
                            <button className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 text-sm">
                              Pay Application Fee
                            </button>
                          </div>
                        )}

                      {application.status === 2 && (
                        <div className="flex items-center gap-2 text-green-600 text-sm font-medium">
                          <CheckCircle className="w-4 h-4" />
                          <span>Your application has been approved!</span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}
