import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';
import './Header.css';

export const Header: React.FC = () => {
  const { user, mode, toggleMode, logout } = useAuth();
  const { items } = useCart();

  const totalCartItems = items.reduce((acc, item) => acc + item.qty, 0);

  return (
    <header className="main-header glass-card">
      <div className="header-brand">
        <span className="logo-icon">🏢</span>
        <h1 className="logo-text">B2B Marketplace</h1>
      </div>

      {user && (
        <div className="header-tenant-info">
          <span className="tenant-badge">Tenant: {user.companyName}</span>
        </div>
      )}

      <div className="header-actions">
        {user && (
          <div className="mode-toggle-container">
            <button 
              className={`mode-toggle-btn ${mode === 'buyer' ? 'active' : ''}`}
              onClick={() => mode !== 'buyer' && toggleMode()}
            >
              Compras
            </button>
            <button 
              className={`mode-toggle-btn ${mode === 'seller' ? 'active' : ''}`}
              onClick={() => mode !== 'seller' && toggleMode()}
            >
              Vendas
            </button>
          </div>
        )}

        {user && mode === 'buyer' && (
          <div className="cart-badge-wrapper">
            <span className="cart-icon">🛒</span>
            {totalCartItems > 0 && <span className="cart-count">{totalCartItems}</span>}
          </div>
        )}

        {user && (
          <div className="user-profile-wrapper">
            <span className="user-avatar">{user.name[0].toUpperCase()}</span>
            <button className="logout-btn" onClick={logout}>Sair</button>
          </div>
        )}
      </div>
    </header>
  );
};
