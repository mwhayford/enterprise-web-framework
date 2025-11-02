import axios from 'axios'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111'

export interface ApplicationDataDto {
  firstName: string
  lastName: string
  email: string
  phone: string
  dateOfBirth: string
  employerName: string
  jobTitle: string
  annualIncome: number
  yearsEmployed: number
  previousAddresses: PreviousAddressDto[]
  references: ReferenceDto[]
  additionalNotes?: string
  termsAccepted: boolean
}

export interface PreviousAddressDto {
  address: string
  moveInDate: string
  moveOutDate: string
  landlordName: string
  landlordPhone: string
  monthlyRent: number
  reasonForLeaving: string
}

export interface ReferenceDto {
  name: string
  relationship: string
  phone: string
  email: string
}

export interface SubmitApplicationDto {
  propertyId: string
  applicationData: ApplicationDataDto
}

export interface PropertyApplicationDto {
  id: string
  propertyId: string
  applicantId: string
  status: number
  applicationData: string
  applicationFee: number
  applicationFeeCurrency: string
  applicationFeePaymentId?: string
  submittedAt?: string
  reviewedAt?: string
  reviewedBy?: string
  decisionNotes?: string
  createdAt: string
  updatedAt: string
}

export interface DecisionRequest {
  decisionNotes?: string
}

export const applicationService = {
  async submitApplication(
    data: SubmitApplicationDto
  ): Promise<PropertyApplicationDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/applications`,
      data,
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
      }
    )
    return response.data
  },

  async getMyApplications(): Promise<PropertyApplicationDto[]> {
    const response = await axios.get(`${API_BASE_URL}/api/applications/my`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    })
    return response.data
  },

  async getApplicationById(id: string): Promise<PropertyApplicationDto> {
    const response = await axios.get(`${API_BASE_URL}/api/applications/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    })
    return response.data
  },

  async approveApplication(
    id: string,
    decisionNotes?: string
  ): Promise<PropertyApplicationDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/applications/${id}/approve`,
      { decisionNotes } as DecisionRequest,
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
      }
    )
    return response.data
  },

  async rejectApplication(
    id: string,
    decisionNotes?: string
  ): Promise<PropertyApplicationDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/applications/${id}/reject`,
      { decisionNotes } as DecisionRequest,
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
      }
    )
    return response.data
  },
}
