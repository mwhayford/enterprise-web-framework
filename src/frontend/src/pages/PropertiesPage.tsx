import { useState, useEffect } from 'react'
import { Filter, ChevronLeft, ChevronRight } from 'lucide-react'
import { PropertyCard } from '../components/properties/PropertyCard'
import { PropertyFilters } from '../components/properties/PropertyFilters'
import {
  propertyService,
  type PropertyFilters as PropertyFiltersType,
} from '../services/propertyService'
import type { PropertyListDto, PagedResult } from '../services/propertyService'

export const PropertiesPage = () => {
  const [properties, setProperties] =
    useState<PagedResult<PropertyListDto> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filters, setFilters] = useState<PropertyFiltersType>({
    pageNumber: 1,
    pageSize: 12,
  })
  const [showMobileFilters, setShowMobileFilters] = useState(false)

  useEffect(() => {
    loadProperties()
  }, [filters.pageNumber])

  const loadProperties = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await propertyService.getAvailableProperties(filters)
      setProperties(data)
    } catch (err) {
      setError('Failed to load properties. Please try again later.')
      console.error('Error loading properties:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleFilterChange = (newFilters: PropertyFiltersType) => {
    setFilters({ ...newFilters, pageNumber: 1, pageSize: 12 })
    loadProperties()
  }

  const handlePageChange = (newPage: number) => {
    setFilters({ ...filters, pageNumber: newPage })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-3xl font-bold text-gray-900">
            Available Properties
          </h1>
          <p className="mt-2 text-gray-600">
            {properties
              ? `${properties.totalCount} properties available`
              : 'Loading...'}
          </p>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex gap-6">
          {/* Filters Sidebar - Desktop */}
          <aside className="hidden lg:block w-80 flex-shrink-0">
            <PropertyFilters onFilterChange={handleFilterChange} />
          </aside>

          {/* Mobile Filter Button */}
          <button
            onClick={() => setShowMobileFilters(true)}
            className="lg:hidden fixed bottom-4 right-4 bg-blue-600 text-white p-4 rounded-full shadow-lg z-10 hover:bg-blue-700"
            aria-label="Open filters"
          >
            <Filter className="w-6 h-6" />
          </button>

          {/* Mobile Filter Overlay */}
          {showMobileFilters && (
            <div className="lg:hidden fixed inset-0 bg-black bg-opacity-50 z-50">
              <div className="bg-white h-full w-full overflow-y-auto">
                <PropertyFilters
                  onFilterChange={handleFilterChange}
                  onClose={() => setShowMobileFilters(false)}
                  isMobile
                />
              </div>
            </div>
          )}

          {/* Properties Grid */}
          <div className="flex-1">
            {loading && (
              <div className="flex items-center justify-center h-64">
                <div className="text-gray-600">Loading properties...</div>
              </div>
            )}

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
                {error}
              </div>
            )}

            {!loading &&
              !error &&
              properties &&
              properties.items.length === 0 && (
                <div className="text-center py-12">
                  <p className="text-gray-600 text-lg">
                    No properties found matching your criteria.
                  </p>
                  <p className="text-gray-500 mt-2">
                    Try adjusting your filters.
                  </p>
                </div>
              )}

            {!loading &&
              !error &&
              properties &&
              properties.items.length > 0 && (
                <>
                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                    {properties.items.map(property => (
                      <PropertyCard key={property.id} property={property} />
                    ))}
                  </div>

                  {/* Pagination */}
                  {properties.totalPages > 1 && (
                    <div className="mt-8 flex items-center justify-center gap-2">
                      <button
                        onClick={() =>
                          handlePageChange(properties.pageNumber - 1)
                        }
                        disabled={!properties.hasPreviousPage}
                        className="p-2 rounded-md border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                        aria-label="Previous page"
                      >
                        <ChevronLeft className="w-5 h-5" />
                      </button>

                      <div className="flex items-center gap-2">
                        {Array.from(
                          { length: Math.min(5, properties.totalPages) },
                          (_, i) => {
                            const page = i + 1
                            return (
                              <button
                                key={page}
                                onClick={() => handlePageChange(page)}
                                className={`px-4 py-2 rounded-md ${
                                  page === properties.pageNumber
                                    ? 'bg-blue-600 text-white'
                                    : 'border border-gray-300 hover:bg-gray-50'
                                }`}
                              >
                                {page}
                              </button>
                            )
                          }
                        )}
                        {properties.totalPages > 5 && (
                          <>
                            <span className="px-2">...</span>
                            <button
                              onClick={() =>
                                handlePageChange(properties.totalPages)
                              }
                              className={`px-4 py-2 rounded-md ${
                                properties.totalPages === properties.pageNumber
                                  ? 'bg-blue-600 text-white'
                                  : 'border border-gray-300 hover:bg-gray-50'
                              }`}
                            >
                              {properties.totalPages}
                            </button>
                          </>
                        )}
                      </div>

                      <button
                        onClick={() =>
                          handlePageChange(properties.pageNumber + 1)
                        }
                        disabled={!properties.hasNextPage}
                        className="p-2 rounded-md border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                        aria-label="Next page"
                      >
                        <ChevronRight className="w-5 h-5" />
                      </button>
                    </div>
                  )}
                </>
              )}
          </div>
        </div>
      </div>
    </div>
  )
}
