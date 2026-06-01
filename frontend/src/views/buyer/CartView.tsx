import React, { useCallback } from 'react';
import { useCart } from '../../contexts/CartContext';
import { ordersService } from '../../services/orders';
import './CartView.css';

export const CartView: React.FC = () => {
  const { items, updateQty, removeFromCart, cartTotal, cartOriginalTotal, cartSavings, clearCart } = useCart();

  const handleCheckout = useCallback(async () => {
    if (items.length === 0) return;
    try {
      const orderPayload = items.map((item) => ({
        productId: item.id,
        qty: item.qty,
      }));
      await ordersService.create(orderPayload);
      clearCart();
      alert('Pedido de compra criado com sucesso! O vendedor foi notificado.');
    } catch (err: any) {
      alert(err.message || 'Erro ao processar pedido.');
    }
  }, [items, clearCart]);

  if (items.length === 0) {
    return (
      <div className="cart-view-empty glass-card">
        <h2>Seu carrinho corporativo está vazio</h2>
        <p>Navegue pelo catálogo para adicionar insumos e lotes atacadistas.</p>
      </div>
    );
  }

  return (
    <div className="cart-view">
      <h2 className="section-title">Carrinho de Compras B2B</h2>
      <div className="cart-content">
        <div className="cart-items-list">
          {items.map((item) => {
            const hasB2BRule = item.b2bMinQty > 0;
            const isB2BApplied = hasB2BRule && item.qty >= item.b2bMinQty;

            return (
              <div key={item.id} className="cart-item glass-card">
                <div className="item-details">
                  <span className="seller-name">🏢 {item.sellerCompanyName}</span>
                  <h3>{item.name}</h3>
                  <div className="item-unit-price">
                    {isB2BApplied ? (
                      <>
                        <span className="old-price">R$ {item.price.toFixed(2)}</span>
                        <span className="active-b2b-price">R$ {item.b2bPrice.toFixed(2)} un (Atacado)</span>
                      </>
                    ) : (
                      <span>R$ {item.price.toFixed(2)} un</span>
                    )}
                  </div>
                </div>

                <div className="item-actions">
                  {hasB2BRule && !isB2BApplied && (
                    <span className="b2b-alert">
                      Adicione mais {item.b2bMinQty - item.qty} un para aplicar R$ {item.b2bPrice.toFixed(2)} unitário!
                    </span>
                  )}
                  <div className="qty-picker">
                    <button onClick={() => updateQty(item.id, item.qty - 1)}>-</button>
                    <input type="number" value={item.qty} readOnly />
                    <button onClick={() => updateQty(item.id, item.qty + 1)}>+</button>
                  </div>
                  <button className="remove-item-btn" onClick={() => removeFromCart(item.id)}>Remover</button>
                </div>
              </div>
            );
          })}
        </div>

        <div className="cart-summary glass-card">
          <h3>Resumo do Pedido</h3>
          <div className="summary-row">
            <span>Total Bruto:</span>
            <span>R$ {cartOriginalTotal.toFixed(2)}</span>
          </div>
          {cartSavings > 0 && (
            <div className="summary-row savings">
              <span>Economia B2B:</span>
              <span>- R$ {cartSavings.toFixed(2)}</span>
            </div>
          )}
          <div className="summary-row total">
            <span>Total a Pagar:</span>
            <span>R$ {cartTotal.toFixed(2)}</span>
          </div>
          <button className="btn-primary checkout-btn" onClick={handleCheckout}>
            Fechar Pedido de Compra
          </button>
        </div>
      </div>
    </div>
  );
};
