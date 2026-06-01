import React, { useMemo } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';
import './Header.css';

/**
 * Componente Header principal do Marketplace B2B.
 * Apresenta a logo da plataforma, informações do inquilino (Tenant) atual,
 * alternador de modo (Compras/Vendas) com acessibilidade aprimorada,
 * contador do carrinho otimizado e informações do perfil do usuário com logout.
 */
export const Header: React.FC = () => {
  const { user, mode, toggleMode, logout } = useAuth();
  const { items } = useCart();

  // Memoiza a soma da quantidade total de itens do carrinho para evitar recálculos desnecessários a cada renderização.
  const totalCartItems = useMemo(() => {
    return items.reduce((acc, item) => acc + item.qty, 0);
  }, [items]);

  return (
    <header className="main-header glass-card">
      <div className="header-brand">
        <span className="logo-icon" role="img" aria-label="Ícone de Prédio Comercial">🏢</span>
        <h1 className="logo-text">B2B Marketplace</h1>
      </div>

      {user && (
        <div className="header-tenant-info">
          <span className="tenant-badge">Tenant: {user.companyName}</span>
        </div>
      )}

      <div className="header-actions">
        {user && (
          <div 
            className="mode-toggle-container" 
            role="tablist" 
            aria-label="Modo de navegação do usuário"
          >
            <button 
              role="tab"
              aria-selected={mode === 'buyer'}
              className={`mode-toggle-btn ${mode === 'buyer' ? 'active' : ''}`}
              onClick={() => mode !== 'buyer' && toggleMode()}
            >
              Compras
            </button>
            <button 
              role="tab"
              aria-selected={mode === 'seller'}
              className={`mode-toggle-btn ${mode === 'seller' ? 'active' : ''}`}
              onClick={() => mode !== 'seller' && toggleMode()}
            >
              Vendas
            </button>
          </div>
        )}

        {user && mode === 'buyer' && (
          <div className="cart-badge-wrapper" role="button" aria-label="Carrinho de compras">
            <span className="cart-icon" role="img" aria-label="Carrinho">🛒</span>
            {totalCartItems > 0 && (
              <span className="cart-count" aria-label={`${totalCartItems} itens no carrinho`}>
                {totalCartItems}
              </span>
            )}
          </div>
        )}

        {user && (
          <div className="user-profile-wrapper">
            <span className="user-avatar" aria-label={`Avatar do usuário com a inicial ${user.name[0]}`}>
              {user.name[0].toUpperCase()}
            </span>
            <button className="logout-btn" onClick={logout}>Sair</button>
          </div>
        )}
      </div>
    </header>
  );
};

