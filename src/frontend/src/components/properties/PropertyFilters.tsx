import { useState } from 'react'
import { X } from 'lucide-react'
import type { PropertyFilters as PropertyFiltersType } from '../../services/propertyService'

interface PropertyFiltersProps {
  onFilterChange: (filters: PropertyFiltersType) => void
  onClose?: () => void
  isMobile?: boolean
}

export const PropertyFilters = ({
  onFilterChange,
  onClose,
  isMobile = false,
}: PropertyFiltersProps) => {
  const [filters, setFilters] = useState<PropertyFiltersType>({
    minBedrooms: undefined,
    maxBedrooms: undefined,
    minBathrooms: undefined,
    maxBathrooms: undefined,
    minRent: undefined,
    maxRent: undefined,
    city: undefined,
    state: undefined,
    propertyType: undefined,
    searchTerm: undefined,
  })

  const handleInputChange = (
    field: keyof PropertyFiltersType,
    value: string
  ) => {
    const numValue = value === '' ? undefined : Number(value)
    const newFilters = {
      ...filters,
      [field]:
        field === 'city' || field === 'state' || field === 'searchTerm'
          ? value || undefined
          : numValue,
    }
    setFilters(newFilters)
  }

  const handleApplyFilters = () => {
    onFilterChange(filters)
    if (isMobile && onClose) {
      onClose()
    }
  }

  const handleClearFilters = () => {
    const clearedFilters: PropertyFiltersType = {}
    setFilters(clearedFilters)
    onFilterChange(clearedFilters)
  }

  return (
    <div
      className={`bg-white ${isMobile ? 'h-full' : 'rounded-lg shadow-md'} p-6`}
    >
      {isMobile && (
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-bold">Filters</h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-gray-100 rounded-full"
            aria-label="Close filters"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
      )}

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Search
          </label>
          <input
            type="text"
            value={filters.searchTerm || ''}
            onChange={e => handleInputChange('searchTerm', e.target.value)}
            placeholder="Search properties..."
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Property Type
          </label>
          <select
            value={filters.propertyType ?? ''}
            onChange={e => handleInputChange('propertyType', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            aria-label="Property Type"
          >
            <option value="">All Types</option>
            <option value="0">Apartment</option>
            <option value="1">House</option>
            <option value="2">Condo</option>
            <option value="3">Townhouse</option>
            <option value="4">Studio</option>
            <option value="5">Other</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Bedrooms
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              min="0"
              value={filters.minBedrooms ?? ''}
              onChange={e => handleInputChange('minBedrooms', e.target.value)}
              placeholder="Min"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
            <input
              type="number"
              min="0"
              value={filters.maxBedrooms ?? ''}
              onChange={e => handleInputChange('maxBedrooms', e.target.value)}
              placeholder="Max"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Bathrooms
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              min="0"
              step="0.5"
              value={filters.minBathrooms ?? ''}
              onChange={e => handleInputChange('minBathrooms', e.target.value)}
              placeholder="Min"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
            <input
              type="number"
              min="0"
              step="0.5"
              value={filters.maxBathrooms ?? ''}
              onChange={e => handleInputChange('maxBathrooms', e.target.value)}
              placeholder="Max"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Monthly Rent
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              min="0"
              value={filters.minRent ?? ''}
              onChange={e => handleInputChange('minRent', e.target.value)}
              placeholder="Min $"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
            <input
              type="number"
              min="0"
              value={filters.maxRent ?? ''}
              onChange={e => handleInputChange('maxRent', e.target.value)}
              placeholder="Max $"
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Location
          </label>
          <input
            type="text"
            value={filters.city || ''}
            onChange={e => handleInputChange('city', e.target.value)}
            placeholder="City"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500 mb-2"
          />
          <input
            type="text"
            value={filters.state || ''}
            onChange={e => handleInputChange('state', e.target.value)}
            placeholder="State"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div className="pt-4 space-y-2">
          <button
            onClick={handleApplyFilters}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
          >
            Apply Filters
          </button>
          <button
            onClick={handleClearFilters}
            className="w-full bg-gray-200 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-300 transition-colors"
          >
            Clear All
          </button>
        </div>
      </div>
    </div>
  )
}
