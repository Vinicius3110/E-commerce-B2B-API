import { apiRequest } from './api';

export interface Product {
  id: string;
  name: string;
  description?: string;
  price: number;
  b2bMinQty: number;
  b2bPrice: number;
  companyId: string;
  companyName: string;
  isActive: boolean;
}

export const productsService = {
  listAll: async (params?: Record<string, string>): Promise<Product[]> => {
    const res = await apiRequest<{ items: Product[] }>('/api/products', { params });
    return res.items || [];
  },
  
  getById: async (id: string): Promise<Product> => {
    return apiRequest<Product>(`/api/products/${id}`);
  },

  create: async (payload: any): Promise<Product> => {
    return apiRequest<Product>('/api/products', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, payload: any): Promise<Product> => {
    return apiRequest<Product>(`/api/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiRequest<void>(`/api/products/${id}`, { method: 'DELETE' });
  },
};
