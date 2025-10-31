import axios from 'axios'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111'

export const LeaseStatus = {
  Draft: 1,
  Active: 2,
  Expired: 3,
  Terminated: 4,
  Renewed: 5,
} as const

export const PaymentFrequency = {
  Monthly: 1,
  Quarterly: 2,
  SemiAnnually: 3,
  Annually: 4,
} as const

export type LeaseStatusType = (typeof LeaseStatus)[keyof typeof LeaseStatus]
export type PaymentFrequencyType =
  (typeof PaymentFrequency)[keyof typeof PaymentFrequency]

export interface LeaseDto {
  id: string
  propertyId: string
  tenantId: string
  landlordId: string
  startDate: string
  endDate: string
  monthlyRent: number
  rentCurrency: string
  securityDeposit: number
  securityDepositCurrency: string
  paymentFrequency: PaymentFrequencyType
  paymentDayOfMonth: number
  status: LeaseStatusType
  specialTerms?: string
  activatedAt?: string
  terminatedAt?: string
  terminationReason?: string
  propertyApplicationId?: string
  durationInDays: number
  remainingDays: number
  isActive: boolean
  isExpired: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateLeaseDto {
  propertyId: string
  tenantId: string
  landlordId: string
  startDate: string
  endDate: string
  monthlyRent: number
  rentCurrency?: string
  securityDeposit: number
  securityDepositCurrency?: string
  paymentFrequency?: PaymentFrequencyType
  paymentDayOfMonth?: number
  specialTerms?: string
  propertyApplicationId?: string
}

export interface UpdateLeaseRentDto {
  monthlyRent: number
  currency?: string
}

export interface RenewLeaseDto {
  newEndDate: string
  newMonthlyRent?: number
  currency?: string
}

export interface TerminateLeaseRequest {
  reason: string
}

const getAuthHeaders = () => ({
  headers: {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
  },
})

export const leaseService = {
  async getLeaseById(id: string): Promise<LeaseDto> {
    const response = await axios.get(
      `${API_BASE_URL}/api/leases/${id}`,
      getAuthHeaders()
    )
    return response.data
  },

  async getLeasesByPropertyId(propertyId: string): Promise<LeaseDto[]> {
    const response = await axios.get(
      `${API_BASE_URL}/api/leases/property/${propertyId}`,
      getAuthHeaders()
    )
    return response.data
  },

  async getLeasesByTenantId(tenantId: string): Promise<LeaseDto[]> {
    const response = await axios.get(
      `${API_BASE_URL}/api/leases/tenant/${tenantId}`,
      getAuthHeaders()
    )
    return response.data
  },

  async getActiveLeaseByPropertyId(
    propertyId: string
  ): Promise<LeaseDto | null> {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/api/leases/property/${propertyId}/active`,
        getAuthHeaders()
      )
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null
      }
      throw error
    }
  },

  async createLease(data: CreateLeaseDto): Promise<LeaseDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/leases`,
      data,
      getAuthHeaders()
    )
    return response.data
  },

  async activateLease(id: string): Promise<LeaseDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/leases/${id}/activate`,
      {},
      getAuthHeaders()
    )
    return response.data
  },

  async terminateLease(id: string, reason: string): Promise<LeaseDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/leases/${id}/terminate`,
      { reason },
      getAuthHeaders()
    )
    return response.data
  },

  async renewLease(id: string, data: RenewLeaseDto): Promise<LeaseDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/leases/${id}/renew`,
      data,
      getAuthHeaders()
    )
    return response.data
  },

  async updateLeaseRent(
    id: string,
    data: UpdateLeaseRentDto
  ): Promise<LeaseDto> {
    const response = await axios.put(
      `${API_BASE_URL}/api/leases/${id}/rent`,
      data,
      getAuthHeaders()
    )
    return response.data
  },
}
