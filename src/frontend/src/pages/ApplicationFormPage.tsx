import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, ChevronRight, ChevronLeft, Plus, X } from 'lucide-react'
import { propertyService, type PropertyDto } from '../services/propertyService'
import {
  applicationService,
  type ApplicationDataDto,
  type PreviousAddressDto,
  type ReferenceDto,
} from '../services/applicationService'
import { applicationSettingsService } from '../services/applicationSettingsService'

export const ApplicationFormPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [property, setProperty] = useState<PropertyDto | null>(null)
  const [applicationFee, setApplicationFee] = useState<number>(35)
  const [currentStep, setCurrentStep] = useState(1)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Form data
  const [formData, setFormData] = useState<ApplicationDataDto>({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    dateOfBirth: '',
    employerName: '',
    jobTitle: '',
    annualIncome: 0,
    yearsEmployed: 0,
    previousAddresses: [],
    references: [],
    additionalNotes: '',
    termsAccepted: false,
  })

  useEffect(() => {
    if (id) {
      loadProperty()
      loadApplicationSettings()
    }
  }, [id])

  const loadProperty = async () => {
    if (!id) return
    try {
      const data = await propertyService.getPropertyById(id)
      setProperty(data)
    } catch (err) {
      setError('Failed to load property details.')
      console.error('Error loading property:', err)
    }
  }

  const loadApplicationSettings = async () => {
    try {
      const settings = await applicationSettingsService.getSettings()
      setApplicationFee(settings.defaultApplicationFee)
    } catch (err) {
      console.error('Error loading application settings:', err)
    }
  }

  const handleInputChange = (
    field: keyof ApplicationDataDto,
    value: string | number | boolean
  ) => {
    setFormData({ ...formData, [field]: value })
  }

  const addPreviousAddress = () => {
    setFormData({
      ...formData,
      previousAddresses: [
        ...formData.previousAddresses,
        {
          address: '',
          moveInDate: '',
          moveOutDate: '',
          landlordName: '',
          landlordPhone: '',
          monthlyRent: 0,
          reasonForLeaving: '',
        },
      ],
    })
  }

  const updatePreviousAddress = (
    index: number,
    field: keyof PreviousAddressDto,
    value: string | number
  ) => {
    const updated = [...formData.previousAddresses]
    updated[index] = { ...updated[index], [field]: value }
    setFormData({ ...formData, previousAddresses: updated })
  }

  const removePreviousAddress = (index: number) => {
    setFormData({
      ...formData,
      previousAddresses: formData.previousAddresses.filter(
        (_, i) => i !== index
      ),
    })
  }

  const addReference = () => {
    setFormData({
      ...formData,
      references: [
        ...formData.references,
        { name: '', relationship: '', phone: '', email: '' },
      ],
    })
  }

  const updateReference = (
    index: number,
    field: keyof ReferenceDto,
    value: string
  ) => {
    const updated = [...formData.references]
    updated[index] = { ...updated[index], [field]: value }
    setFormData({ ...formData, references: updated })
  }

  const removeReference = (index: number) => {
    setFormData({
      ...formData,
      references: formData.references.filter((_, i) => i !== index),
    })
  }

  const handleSubmit = async () => {
    if (!id || !property) return

    if (!formData.termsAccepted) {
      setError(
        'You must accept the terms and conditions to submit your application.'
      )
      return
    }

    try {
      setLoading(true)
      setError(null)

      await applicationService.submitApplication({
        propertyId: id,
        applicationData: formData,
      })

      navigate('/applications/my', {
        state: { message: 'Application submitted successfully!' },
      })
    } catch (err) {
      setError('Failed to submit application. Please try again.')
      console.error('Error submitting application:', err)
    } finally {
      setLoading(false)
    }
  }

  const nextStep = () => {
    if (currentStep < 4) setCurrentStep(currentStep + 1)
  }

  const prevStep = () => {
    if (currentStep > 1) setCurrentStep(currentStep - 1)
  }

  if (!property) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-600">Loading...</div>
      </div>
    )
  }

  const displayApplicationFee = property.applicationFee ?? applicationFee

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <button
            onClick={() => navigate(`/properties/${id}`)}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
          >
            <ArrowLeft className="w-5 h-5" />
            <span>Back to Property</span>
          </button>
          <h1 className="text-2xl font-bold text-gray-900">
            Rental Application
          </h1>
          <p className="text-gray-600 mt-1">
            {property.street}, {property.city}
          </p>
        </div>
      </div>

      {/* Progress Bar */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            {['Personal Info', 'Employment', 'Rental History', 'Review'].map(
              (step, index) => (
                <div key={step} className="flex items-center">
                  <div
                    className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${
                      index + 1 <= currentStep
                        ? 'bg-blue-600 text-white'
                        : 'bg-gray-200 text-gray-600'
                    }`}
                  >
                    {index + 1}
                  </div>
                  <span
                    className={`ml-2 text-sm ${index + 1 <= currentStep ? 'text-gray-900' : 'text-gray-500'}`}
                  >
                    {step}
                  </span>
                  {index < 3 && (
                    <ChevronRight className="w-5 h-5 text-gray-400 mx-4" />
                  )}
                </div>
              )
            )}
          </div>
        </div>
      </div>

      {/* Form Content */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white rounded-lg shadow-md p-6">
          {error && (
            <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
              {error}
            </div>
          )}

          {/* Step 1: Personal Information */}
          {currentStep === 1 && (
            <div className="space-y-4">
              <h2 className="text-xl font-bold mb-4">Personal Information</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    First Name *
                  </label>
                  <input
                    type="text"
                    value={formData.firstName}
                    onChange={e =>
                      handleInputChange('firstName', e.target.value)
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="First name"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Last Name *
                  </label>
                  <input
                    type="text"
                    value={formData.lastName}
                    onChange={e =>
                      handleInputChange('lastName', e.target.value)
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Last name"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Email *
                  </label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={e => handleInputChange('email', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Email address"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Phone *
                  </label>
                  <input
                    type="tel"
                    value={formData.phone}
                    onChange={e => handleInputChange('phone', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Phone number"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Date of Birth *
                  </label>
                  <input
                    type="date"
                    value={formData.dateOfBirth}
                    onChange={e =>
                      handleInputChange('dateOfBirth', e.target.value)
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Date of birth"
                    required
                  />
                </div>
              </div>
            </div>
          )}

          {/* Step 2: Employment */}
          {currentStep === 2 && (
            <div className="space-y-4">
              <h2 className="text-xl font-bold mb-4">Employment Information</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Employer Name *
                  </label>
                  <input
                    type="text"
                    value={formData.employerName}
                    onChange={e =>
                      handleInputChange('employerName', e.target.value)
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Employer name"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Job Title *
                  </label>
                  <input
                    type="text"
                    value={formData.jobTitle}
                    onChange={e =>
                      handleInputChange('jobTitle', e.target.value)
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Job title"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Annual Income *
                  </label>
                  <input
                    type="number"
                    value={formData.annualIncome}
                    onChange={e =>
                      handleInputChange('annualIncome', Number(e.target.value))
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Annual income"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Years Employed *
                  </label>
                  <input
                    type="number"
                    value={formData.yearsEmployed}
                    onChange={e =>
                      handleInputChange('yearsEmployed', Number(e.target.value))
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    aria-label="Years employed"
                    required
                  />
                </div>
              </div>
            </div>
          )}

          {/* Step 3: Rental History */}
          {currentStep === 3 && (
            <div className="space-y-6">
              <div className="flex items-center justify-between">
                <h2 className="text-xl font-bold">
                  Rental History & References
                </h2>
              </div>

              <div>
                <div className="flex items-center justify-between mb-3">
                  <h3 className="font-semibold">Previous Addresses</h3>
                  <button
                    type="button"
                    onClick={addPreviousAddress}
                    className="flex items-center gap-1 text-blue-600 hover:text-blue-700"
                  >
                    <Plus className="w-4 h-4" />
                    <span>Add Address</span>
                  </button>
                </div>
                {formData.previousAddresses.map((addr, index) => (
                  <div
                    key={index}
                    className="border rounded-lg p-4 mb-4 relative"
                  >
                    <button
                      type="button"
                      onClick={() => removePreviousAddress(index)}
                      className="absolute top-2 right-2 text-red-600 hover:text-red-700"
                      aria-label="Remove address"
                    >
                      <X className="w-5 h-5" />
                    </button>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Address
                        </label>
                        <input
                          type="text"
                          value={addr.address}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'address',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Previous address"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Move In Date
                        </label>
                        <input
                          type="date"
                          value={addr.moveInDate}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'moveInDate',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Move in date"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Move Out Date
                        </label>
                        <input
                          type="date"
                          value={addr.moveOutDate}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'moveOutDate',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Move out date"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Landlord Name
                        </label>
                        <input
                          type="text"
                          value={addr.landlordName}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'landlordName',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Landlord name"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Landlord Phone
                        </label>
                        <input
                          type="tel"
                          value={addr.landlordPhone}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'landlordPhone',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Landlord phone"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Monthly Rent
                        </label>
                        <input
                          type="number"
                          value={addr.monthlyRent}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'monthlyRent',
                              Number(e.target.value)
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Monthly rent"
                        />
                      </div>
                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Reason for Leaving
                        </label>
                        <input
                          type="text"
                          value={addr.reasonForLeaving}
                          onChange={e =>
                            updatePreviousAddress(
                              index,
                              'reasonForLeaving',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Reason for leaving"
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              <div>
                <div className="flex items-center justify-between mb-3">
                  <h3 className="font-semibold">References</h3>
                  <button
                    type="button"
                    onClick={addReference}
                    className="flex items-center gap-1 text-blue-600 hover:text-blue-700"
                  >
                    <Plus className="w-4 h-4" />
                    <span>Add Reference</span>
                  </button>
                </div>
                {formData.references.map((ref, index) => (
                  <div
                    key={index}
                    className="border rounded-lg p-4 mb-4 relative"
                  >
                    <button
                      type="button"
                      onClick={() => removeReference(index)}
                      className="absolute top-2 right-2 text-red-600 hover:text-red-700"
                      aria-label="Remove reference"
                    >
                      <X className="w-5 h-5" />
                    </button>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Name
                        </label>
                        <input
                          type="text"
                          value={ref.name}
                          onChange={e =>
                            updateReference(index, 'name', e.target.value)
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Reference name"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Relationship
                        </label>
                        <input
                          type="text"
                          value={ref.relationship}
                          onChange={e =>
                            updateReference(
                              index,
                              'relationship',
                              e.target.value
                            )
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Relationship"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Phone
                        </label>
                        <input
                          type="tel"
                          value={ref.phone}
                          onChange={e =>
                            updateReference(index, 'phone', e.target.value)
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Reference phone"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Email
                        </label>
                        <input
                          type="email"
                          value={ref.email}
                          onChange={e =>
                            updateReference(index, 'email', e.target.value)
                          }
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                          aria-label="Reference email"
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Additional Notes (Optional)
                </label>
                <textarea
                  value={formData.additionalNotes}
                  onChange={e =>
                    handleInputChange('additionalNotes', e.target.value)
                  }
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Any additional information you'd like to provide..."
                />
              </div>
            </div>
          )}

          {/* Step 4: Review & Submit */}
          {currentStep === 4 && (
            <div className="space-y-6">
              <h2 className="text-xl font-bold mb-4">Review & Submit</h2>

              <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                <div>
                  <h3 className="font-semibold mb-2">Personal Information</h3>
                  <p className="text-sm text-gray-700">
                    {formData.firstName} {formData.lastName}
                    <br />
                    {formData.email}
                    <br />
                    {formData.phone}
                    <br />
                    DOB: {formData.dateOfBirth}
                  </p>
                </div>

                <div>
                  <h3 className="font-semibold mb-2">Employment</h3>
                  <p className="text-sm text-gray-700">
                    {formData.jobTitle} at {formData.employerName}
                    <br />
                    Annual Income: ${formData.annualIncome.toLocaleString()}
                    <br />
                    Years Employed: {formData.yearsEmployed}
                  </p>
                </div>

                <div>
                  <h3 className="font-semibold mb-2">Application Fee</h3>
                  <p className="text-lg font-bold text-blue-600">
                    ${displayApplicationFee.toLocaleString()} USD
                  </p>
                  <p className="text-sm text-gray-600 mt-1">
                    Payment will be processed after submission
                  </p>
                </div>
              </div>

              <div className="flex items-start gap-2">
                <input
                  type="checkbox"
                  id="terms"
                  checked={formData.termsAccepted}
                  onChange={e =>
                    handleInputChange('termsAccepted', e.target.checked)
                  }
                  className="mt-1"
                />
                <label htmlFor="terms" className="text-sm text-gray-700">
                  I certify that all information provided in this application is
                  true and complete. I authorize the landlord to verify this
                  information and conduct background checks as necessary.
                </label>
              </div>
            </div>
          )}

          {/* Navigation Buttons */}
          <div className="flex items-center justify-between mt-8 pt-6 border-t">
            <button
              onClick={prevStep}
              disabled={currentStep === 1}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronLeft className="w-5 h-5" />
              <span>Previous</span>
            </button>

            {currentStep < 4 ? (
              <button
                onClick={nextStep}
                className="flex items-center gap-2 px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                <span>Next</span>
                <ChevronRight className="w-5 h-5" />
              </button>
            ) : (
              <button
                onClick={handleSubmit}
                disabled={loading || !formData.termsAccepted}
                className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Submitting...' : 'Submit Application'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
