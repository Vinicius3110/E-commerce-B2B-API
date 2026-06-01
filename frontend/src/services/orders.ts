import { apiRequest } from './api';

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  price: number;
  qty: number;
}

export interface Order {
  id: string;
  buyerCompanyId: string;
  buyerCompanyName: string;
  sellerCompanyId: string;
  sellerCompanyName: string;
  status: 'Pendente' | 'Confirmado' | 'Enviado' | 'Entregue' | 'Cancelado';
  total: number;
  createdAt: string;
  items: OrderItem[];
}

export const ordersService = {
  list: async (role: 'buyer' | 'seller'): Promise<Order[]> => {
    const res = await apiRequest<{ items: Order[] }>('/api/orders', {
      params: { role },
    });
    return res.items || [];
  },

  create: async (items: { productId: string; qty: number }[]): Promise<Order> => {
    return apiRequest<Order>('/api/orders', {
      method: 'POST',
      body: JSON.stringify({ items }),
    });
  },

  updateStatus: async (id: string, status: string): Promise<Order> => {
    return apiRequest<Order>(`/api/orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    });
  },
};
