import React, { useState } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { Header } from './components/layout/Header';

// Views
import { AuthView } from './views/auth/AuthView';
import { ProductsView } from './views/buyer/ProductsView';
import { CartView } from './views/buyer/CartView';
import { OrdersView } from './views/buyer/OrdersView';
import { ProductsViewSeller } from './views/seller/ProductsView';
import { OrdersViewSeller } from './views/seller/OrdersView';
import './App.css';

const MainAppContent: React.FC = () => {
  const { isAuthenticated, mode, loading } = useAuth();
  const [buyerTab, setBuyerTab] = useState<'products' | 'cart' | 'orders'>('products');
  const [sellerTab, setSellerTab] = useState<'products' | 'orders'>('products');

  if (loading) return <div className="app-loading">Carregando sistema B2B...</div>;

  if (!isAuthenticated) {
    return <AuthView />;
  }

  return (
    <div className="app-container">
      <Header />
      
      {/* Navegação Secundária Baseada em Abas para o Modo Comprador */}
      {mode === 'buyer' ? (
        <>
          <div className="sub-navigation">
            <button 
              className={`sub-nav-btn ${buyerTab === 'products' ? 'active' : ''}`}
              onClick={() => setBuyerTab('products')}
            >
              Catálogo
            </button>
            <button 
              className={`sub-nav-btn ${buyerTab === 'cart' ? 'active' : ''}`}
              onClick={() => setBuyerTab('cart')}
            >
              Carrinho
            </button>
            <button 
              className={`sub-nav-btn ${buyerTab === 'orders' ? 'active' : ''}`}
              onClick={() => setBuyerTab('orders')}
            >
              Meus Pedidos
            </button>
          </div>
          
          <main className="main-content">
            {buyerTab === 'products' && <ProductsView />}
            {buyerTab === 'cart' && <CartView />}
            {buyerTab === 'orders' && <OrdersView />}
          </main>
        </>
      ) : (
        /* Navegação Secundária Baseada em Abas para o Modo Vendedor */
        <>
          <div className="sub-navigation">
            <button 
              className={`sub-nav-btn ${sellerTab === 'products' ? 'active' : ''}`}
              onClick={() => setSellerTab('products')}
            >
              Meus Produtos
            </button>
            <button 
              className={`sub-nav-btn ${sellerTab === 'orders' ? 'active' : ''}`}
              onClick={() => setSellerTab('orders')}
            >
              Vendas Recebidas
            </button>
          </div>
          
          <main className="main-content">
            {sellerTab === 'products' && <ProductsViewSeller />}
            {sellerTab === 'orders' && <OrdersViewSeller />}
          </main>
        </>
      )}
    </div>
  );
};

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <MainAppContent />
      </CartProvider>
    </AuthProvider>
  );
}

export default App;
