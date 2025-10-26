import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111';

export interface PropertyFilters {
  pageNumber?: number;
  pageSize?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minBathrooms?: number;
  maxBathrooms?: number;
  minRent?: number;
  maxRent?: number;
  city?: string;
  state?: string;
  propertyType?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PropertyListDto {
  id: string;
  address: string;
  propertyType: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  monthlyRent: number;
  rentCurrency: string;
  availableDate: string;
  status: number;
  thumbnailImage?: string;
  applicationFee?: number;
}

export interface PropertyDto {
  id: string;
  ownerId: string;
  street: string;
  unit?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  propertyType: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  monthlyRent: number;
  rentCurrency: string;
  securityDeposit: number;
  securityDepositCurrency: string;
  availableDate: string;
  status: number;
  description: string;
  applicationFee?: number;
  applicationFeeCurrency?: string;
  amenities: string[];
  images: string[];
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreatePropertyDto {
  street: string;
  unit?: string;
  city: string;
  state: string;
  zipCode: string;
  country?: string;
  propertyType: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  monthlyRent: number;
  rentCurrency?: string;
  securityDeposit: number;
  securityDepositCurrency?: string;
  availableDate: string;
  description: string;
  applicationFee?: number;
  applicationFeeCurrency?: string;
  amenities?: string[];
  images?: string[];
}

export const propertyService = {
  async getAvailableProperties(filters: PropertyFilters = {}): Promise<PagedResult<PropertyListDto>> {
    const params = new URLSearchParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString());
      }
    });

    const response = await axios.get(`${API_BASE_URL}/api/properties?${params.toString()}`);
    return response.data;
  },

  async getPropertyById(id: string): Promise<PropertyDto> {
    const response = await axios.get(`${API_BASE_URL}/api/properties/${id}`);
    return response.data;
  },

  async createProperty(data: CreatePropertyDto): Promise<PropertyDto> {
    const response = await axios.post(`${API_BASE_URL}/api/properties`, data, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    });
    return response.data;
  },

  async updateProperty(id: string, data: CreatePropertyDto): Promise<PropertyDto> {
    const response = await axios.put(`${API_BASE_URL}/api/properties/${id}`, data, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    });
    return response.data;
  },
};

