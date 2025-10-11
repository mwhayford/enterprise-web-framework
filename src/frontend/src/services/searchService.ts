import apiService from './api';
import type { SearchResult } from '../types';

export interface SearchParams {
  query: string;
  index?: string;
  page?: number;
  pageSize?: number;
}

export interface IndexDocumentParams {
  document: any;
  index?: string;
  id?: string;
}

class SearchService {
  async search<T = any>(params: SearchParams): Promise<SearchResult<T>> {
    const { query, index = 'core-index', page = 1, pageSize = 20 } = params;
    
    const response = await apiService.search(query, index, page, pageSize);
    return response;
  }

  async searchUsers(params: Omit<SearchParams, 'index'>): Promise<SearchResult<any>> {
    const { query, page = 1, pageSize = 20 } = params;
    
    const response = await apiService.searchUsers(query, page, pageSize);
    return response;
  }

  async searchPayments(params: Omit<SearchParams, 'index'>): Promise<SearchResult<any>> {
    const { query, page = 1, pageSize = 20 } = params;
    
    const response = await apiService.searchPayments(query, page, pageSize);
    return response;
  }

  async indexDocument(params: IndexDocumentParams): Promise<void> {
    const { document, index = 'core-index', id } = params;
    
    await apiService.indexDocument(document, index, id);
  }
}

export default new SearchService();
