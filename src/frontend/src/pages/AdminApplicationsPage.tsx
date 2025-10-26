import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FileText, Filter, Eye } from 'lucide-react';
import { applicationService, type PropertyApplicationDto } from '../services/applicationService';

const statusLabels: Record<number, string> = {
  0: 'Pending',
  1: 'Under Review',
  2: 'Approved',
  3: 'Rejected',
  4: 'Withdrawn',
};

const statusColors: Record<number, string> = {
  0: 'bg-yellow-100 text-yellow-800',
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-gray-100 text-gray-800',
};

export const AdminApplicationsPage = () => {
  const [applications, setApplications] = useState<PropertyApplicationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<number | null>(null);

  useEffect(() => {
    loadApplications();
  }, []);

  const loadApplications = async () => {
    try {
      setLoading(true);
      setError(null);
      // Note: This would need an admin endpoint in the future
      const data = await applicationService.getMyApplications();
      setApplications(data);
    } catch (err) {
      setError('Failed to load applications. Please try again later.');
      console.error('Error loading applications:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const filteredApplications = filterStatus !== null
    ? applications.filter(app => app.status === filterStatus)
    : applications;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-3xl font-bold text-gray-900">Application Management</h1>
          <p className="mt-2 text-gray-600">
            Review and manage rental applications
          </p>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Filters */}
        <div className="bg-white rounded-lg shadow-md p-4 mb-6">
          <div className="flex items-center gap-4">
            <Filter className="w-5 h-5 text-gray-600" />
            <span className="text-sm font-medium text-gray-700">Filter by status:</span>
            <div className="flex gap-2 flex-wrap">
              <button
                onClick={() => setFilterStatus(null)}
                className={`px-3 py-1 rounded-md text-sm ${
                  filterStatus === null
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                All
              </button>
              {Object.entries(statusLabels).map(([status, label]) => (
                <button
                  key={status}
                  onClick={() => setFilterStatus(Number(status))}
                  className={`px-3 py-1 rounded-md text-sm ${
                    filterStatus === Number(status)
                      ? 'bg-blue-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {loading && (
          <div className="flex items-center justify-center h-64">
            <div className="text-gray-600">Loading applications...</div>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
            {error}
          </div>
        )}

        {!loading && !error && filteredApplications.length === 0 && (
          <div className="bg-white rounded-lg shadow-md p-12 text-center">
            <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-900 mb-2">No Applications Found</h2>
            <p className="text-gray-600">
              {filterStatus !== null
                ? 'No applications with this status.'
                : 'No applications have been submitted yet.'}
            </p>
          </div>
        )}

        {!loading && !error && filteredApplications.length > 0 && (
          <div className="bg-white rounded-lg shadow-md overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Applicant
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Property
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Submitted
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Fee
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredApplications.map((application) => {
                  const applicationData = application.applicationData
                    ? JSON.parse(application.applicationData)
                    : null;

                  return (
                    <tr key={application.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">
                          {applicationData?.firstName} {applicationData?.lastName}
                        </div>
                        <div className="text-sm text-gray-500">{applicationData?.email}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">
                          {application.propertyId.substring(0, 8)}...
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${
                            statusColors[application.status]
                          }`}
                        >
                          {statusLabels[application.status]}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(application.submittedAt)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {application.applicationFeeCurrency} {application.applicationFee.toLocaleString()}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <Link
                          to={`/admin/applications/${application.id}`}
                          className="flex items-center gap-1 text-blue-600 hover:text-blue-900"
                        >
                          <Eye className="w-4 h-4" />
                          <span>Review</span>
                        </Link>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

