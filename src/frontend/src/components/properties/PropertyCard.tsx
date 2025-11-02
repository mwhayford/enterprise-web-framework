import { Link } from 'react-router-dom'
import { Home, Bed, Bath, Square, DollarSign, Calendar } from 'lucide-react'
import type { PropertyListDto } from '../../services/propertyService'

interface PropertyCardProps {
  property: PropertyListDto
}

const propertyTypeLabels: Record<number, string> = {
  0: 'Apartment',
  1: 'House',
  2: 'Condo',
  3: 'Townhouse',
  4: 'Studio',
  5: 'Other',
}

const propertyStatusLabels: Record<number, string> = {
  0: 'Available',
  1: 'Rented',
  2: 'Maintenance',
  3: 'Unlisted',
}

export const PropertyCard = ({ property }: PropertyCardProps) => {
  const availableDate = new Date(property.availableDate).toLocaleDateString(
    'en-US',
    {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }
  )

  const statusColor =
    property.status === 0
      ? 'bg-green-100 text-green-800'
      : 'bg-gray-100 text-gray-800'

  return (
    <Link
      to={`/properties/${property.id}`}
      className="block bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-shadow duration-300"
      data-testid="property-card"
    >
      <div className="relative h-48 bg-gray-200">
        {property.thumbnailImage ? (
          <img
            src={property.thumbnailImage}
            alt={property.address}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <Home className="w-16 h-16 text-gray-400" />
          </div>
        )}
        <div
          className={`absolute top-2 right-2 px-3 py-1 rounded-full text-sm font-semibold ${statusColor}`}
        >
          {propertyStatusLabels[property.status]}
        </div>
      </div>

      <div className="p-4">
        <div className="flex items-start justify-between mb-2">
          <div>
            <h3 className="text-xl font-bold text-gray-900 mb-1">
              {property.rentCurrency} {property.monthlyRent.toLocaleString()}/mo
            </h3>
            <p className="text-gray-600 text-sm">{property.address}</p>
          </div>
          <span className="bg-blue-100 text-blue-800 text-xs font-semibold px-2 py-1 rounded">
            {propertyTypeLabels[property.propertyType]}
          </span>
        </div>

        <div className="flex items-center gap-4 text-gray-600 text-sm mb-3">
          <div className="flex items-center gap-1">
            <Bed className="w-4 h-4" />
            <span>{property.bedrooms} bed</span>
          </div>
          <div className="flex items-center gap-1">
            <Bath className="w-4 h-4" />
            <span>{property.bathrooms} bath</span>
          </div>
          <div className="flex items-center gap-1">
            <Square className="w-4 h-4" />
            <span>{property.squareFeet.toLocaleString()} sq ft</span>
          </div>
        </div>

        <div className="flex items-center justify-between pt-3 border-t border-gray-200">
          <div className="flex items-center gap-1 text-sm text-gray-600">
            <Calendar className="w-4 h-4" />
            <span>Available {availableDate}</span>
          </div>
          {property.applicationFee && (
            <div className="flex items-center gap-1 text-sm text-gray-600">
              <DollarSign className="w-4 h-4" />
              <span>{property.applicationFee} app fee</span>
            </div>
          )}
        </div>
      </div>
    </Link>
  )
}
