import { useState, useEffect, useCallback } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  Bed,
  Bath,
  Square,
  Calendar,
  MapPin,
  Check,
} from 'lucide-react'
import { propertyService, type PropertyDto } from '../services/propertyService'
import { applicationSettingsService } from '../services/applicationSettingsService'
import { PublicLayout } from '../components/layout/PublicLayout'

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

export const PropertyDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [property, setProperty] = useState<PropertyDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedImageIndex, setSelectedImageIndex] = useState(0)
  const [applicationFee, setApplicationFee] = useState<number | null>(null)

  const loadProperty = useCallback(async () => {
    if (!id) return

    try {
      setLoading(true)
      setError(null)
      const data = await propertyService.getPropertyById(id)
      setProperty(data)
    } catch (err) {
      setError('Failed to load property details. Please try again later.')
      console.error('Error loading property:', err)
    } finally {
      setLoading(false)
    }
  }, [id])

  const loadApplicationSettings = useCallback(async () => {
    try {
      const settings = await applicationSettingsService.getSettings()
      setApplicationFee(settings.defaultApplicationFee)
    } catch (err) {
      console.error('Error loading application settings:', err)
    }
  }, [])

  useEffect(() => {
    if (id) {
      loadProperty()
      loadApplicationSettings()
    }
  }, [id, loadProperty, loadApplicationSettings])

  const handleApplyNow = () => {
    if (!property) return

    const token = localStorage.getItem('token')
    if (!token) {
      navigate('/login', { state: { from: `/properties/${id}` } })
      return
    }

    navigate(`/properties/${id}/apply`)
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-600">Loading property details...</div>
      </div>
    )
  }

  if (error || !property) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 text-lg mb-4">
            {error || 'Property not found'}
          </p>
          <button
            onClick={() => navigate('/properties')}
            className="text-blue-600 hover:underline"
          >
            Back to Properties
          </button>
        </div>
      </div>
    )
  }

  const availableDate = new Date(property.availableDate).toLocaleDateString(
    'en-US',
    {
      month: 'long',
      day: 'numeric',
      year: 'numeric',
    }
  )

  const displayApplicationFee = property.applicationFee ?? applicationFee

  return (
    <PublicLayout>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Back Button */}
        <button
          onClick={() => navigate('/properties')}
          className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
        >
          <ArrowLeft className="w-5 h-5" />
          <span>Back to Properties</span>
        </button>
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column - Images and Details */}
          <div className="lg:col-span-2 space-y-6">
            {/* Image Gallery */}
            <div className="bg-white rounded-lg shadow-md overflow-hidden">
              <div className="aspect-video bg-gray-200">
                {property.images && property.images.length > 0 ? (
                  <img
                    src={property.images[selectedImageIndex]}
                    alt={`Property view ${selectedImageIndex + 1}`}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center">
                    <MapPin className="w-16 h-16 text-gray-400" />
                  </div>
                )}
              </div>
              {property.images && property.images.length > 1 && (
                <div className="flex gap-2 p-4 overflow-x-auto">
                  {property.images.map((image, index) => (
                    <button
                      key={index}
                      onClick={() => setSelectedImageIndex(index)}
                      className={`flex-shrink-0 w-20 h-20 rounded-md overflow-hidden border-2 ${
                        index === selectedImageIndex
                          ? 'border-blue-600'
                          : 'border-transparent'
                      }`}
                    >
                      <img
                        src={image}
                        alt={`Thumbnail ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Property Details */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-2xl font-bold mb-4">Property Details</h2>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div className="flex items-center gap-2">
                  <Bed className="w-5 h-5 text-gray-600" />
                  <div>
                    <p className="text-sm text-gray-600">Bedrooms</p>
                    <p className="font-semibold">{property.bedrooms}</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Bath className="w-5 h-5 text-gray-600" />
                  <div>
                    <p className="text-sm text-gray-600">Bathrooms</p>
                    <p className="font-semibold">{property.bathrooms}</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Square className="w-5 h-5 text-gray-600" />
                  <div>
                    <p className="text-sm text-gray-600">Square Feet</p>
                    <p className="font-semibold">
                      {property.squareFeet.toLocaleString()}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="w-5 h-5 text-gray-600" />
                  <div>
                    <p className="text-sm text-gray-600">Available</p>
                    <p className="font-semibold">{availableDate}</p>
                  </div>
                </div>
              </div>

              <div className="border-t pt-6">
                <h3 className="font-semibold text-lg mb-2">Description</h3>
                <p className="text-gray-700 whitespace-pre-line">
                  {property.description}
                </p>
              </div>

              {property.amenities && property.amenities.length > 0 && (
                <div className="border-t pt-6 mt-6">
                  <h3 className="font-semibold text-lg mb-3">Amenities</h3>
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                    {property.amenities.map((amenity, index) => (
                      <div key={index} className="flex items-center gap-2">
                        <Check className="w-4 h-4 text-green-600" />
                        <span className="text-gray-700">{amenity}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              <div className="border-t pt-6 mt-6">
                <h3 className="font-semibold text-lg mb-2">Location</h3>
                <div className="flex items-start gap-2 text-gray-700">
                  <MapPin className="w-5 h-5 text-gray-600 mt-0.5" />
                  <div>
                    <p>
                      {property.street}
                      {property.unit ? `, ${property.unit}` : ''}
                    </p>
                    <p>
                      {property.city}, {property.state} {property.zipCode}
                    </p>
                    <p>{property.country}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Right Column - Application Card */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-md p-6 sticky top-4">
              <div className="mb-4">
                <div className="flex items-baseline gap-2 mb-2">
                  <span className="text-3xl font-bold text-gray-900">
                    {property.rentCurrency}{' '}
                    {property.monthlyRent.toLocaleString()}
                  </span>
                  <span className="text-gray-600">/month</span>
                </div>
                <span className="inline-block bg-blue-100 text-blue-800 text-sm font-semibold px-3 py-1 rounded">
                  {propertyTypeLabels[property.propertyType]}
                </span>
                <span
                  className={`ml-2 inline-block text-sm font-semibold px-3 py-1 rounded ${
                    property.status === 0
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {propertyStatusLabels[property.status]}
                </span>
              </div>

              <div className="space-y-3 mb-6 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600">Security Deposit</span>
                  <span className="font-semibold">
                    {property.securityDepositCurrency}{' '}
                    {property.securityDeposit.toLocaleString()}
                  </span>
                </div>
                {displayApplicationFee && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Application Fee</span>
                    <span className="font-semibold">
                      {property.applicationFeeCurrency || 'USD'}{' '}
                      {displayApplicationFee.toLocaleString()}
                    </span>
                  </div>
                )}
              </div>

              {property.status === 0 ? (
                <button
                  onClick={handleApplyNow}
                  className="w-full bg-blue-600 text-white py-3 px-4 rounded-md hover:bg-blue-700 transition-colors font-semibold"
                >
                  Apply Now
                </button>
              ) : (
                <div className="bg-gray-100 text-gray-600 py-3 px-4 rounded-md text-center">
                  This property is not currently available
                </div>
              )}

              <p className="text-xs text-gray-500 mt-4 text-center">
                By applying, you agree to our terms and conditions
              </p>
            </div>
          </div>
        </div>
      </div>
    </PublicLayout>
  )
}
