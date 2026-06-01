import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { FC, ReactNode } from 'react';
import { useAuth } from './AuthContext';

export interface CartItem {
  id: string;                 // Product ID
  name: string;
  price: number;
  b2bMinQty: number;
  b2bPrice: number;
  sellerCompanyId: string;
  sellerCompanyName: string;
  qty: number;
}

interface CartContextType {
  items: CartItem[];
  addToCart: (product: {
    id: string;
    name: string;
    price: number;
    b2bMinQty?: number;
    b2bPrice?: number;
    companyId: string;
    companyName?: string;
  }, qty: number) => void;
  removeFromCart: (productId: string) => void;
  updateQty: (productId: string, qty: number) => void;
  clearCart: () => void;
  cartTotal: number;
  cartOriginalTotal: number;
  cartSavings: number;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [items, setItems] = useState<CartItem[]>([]);
  const { user } = useAuth();

  // Reset carrinho ao trocar de conta
  useEffect(() => {
    setItems([]);
  }, [user?.id]);

  const addToCart = useCallback((product: {
    id: string;
    name: string;
    price: number;
    b2bMinQty?: number;
    b2bPrice?: number;
    companyId: string;
    companyName?: string;
  }, qty: number) => {
    if (user && product.companyId === user.companyId) {
      alert('Regra B2B: Você não pode comprar produtos cadastrados pela sua própria empresa.');
      return;
    }

    setItems((prev) => {
      const existing = prev.find((item) => item.id === product.id);
      if (existing) {
        return prev.map((item) =>
          item.id === product.id ? { ...item, qty: item.qty + qty } : item
        );
      }
      return [
        ...prev,
        {
          id: product.id,
          name: product.name,
          price: product.price,
          b2bMinQty: product.b2bMinQty || 0,
          b2bPrice: product.b2bPrice || product.price,
          sellerCompanyId: product.companyId,
          sellerCompanyName: product.companyName || 'Empresa B2B',
          qty,
        },
      ];
    });
  }, [user]);

  const removeFromCart = useCallback((productId: string) => {
    setItems((prev) => prev.filter((item) => item.id !== productId));
  }, []);

  const updateQty = useCallback((productId: string, qty: number) => {
    if (qty <= 0) {
      removeFromCart(productId);
      return;
    }
    setItems((prev) =>
      prev.map((item) => (item.id === productId ? { ...item, qty } : item))
    );
  }, [removeFromCart]);

  const clearCart = useCallback(() => {
    setItems([]);
  }, []);

  const cartOriginalTotal = items.reduce((acc, item) => acc + item.price * item.qty, 0);

  const cartTotal = items.reduce((acc, item) => {
    const activePrice = item.b2bMinQty > 0 && item.qty >= item.b2bMinQty ? item.b2bPrice : item.price;
    return acc + activePrice * item.qty;
  }, 0);

  const cartSavings = cartOriginalTotal - cartTotal;

  return (
    <CartContext.Provider value={{
      items,
      addToCart,
      removeFromCart,
      updateQty,
      clearCart,
      cartTotal,
      cartOriginalTotal,
      cartSavings,
    }}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) throw new Error('useCart deve ser usado dentro de um CartProvider');
  return context;
};
