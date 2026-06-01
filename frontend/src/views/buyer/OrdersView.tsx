import React, { useEffect, useState, useCallback } from 'react';
import { ordersService } from '../../services/orders';
import type { Order } from '../../services/orders';
import './OrdersView.css';

export const OrdersView: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

  const loadOrders = useCallback((activeRef: { active: boolean }) => {
    ordersService.list('buyer')
      .then((data) => {
        if (activeRef.active) setOrders(data);
      })
      .finally(() => {
        if (activeRef.active) setLoading(false);
      });
  }, []);

  useEffect(() => {
    const activeRef = { active: true };
    loadOrders(activeRef);
    return () => {
      activeRef.active = false;
    };
  }, [loadOrders]);

  const handleCancelOrder = useCallback(async (orderId: string) => {
    if (!confirm('Deseja realmente cancelar este pedido?')) return;
    try {
      await ordersService.updateStatus(orderId, 'Cancelado');
      alert('Pedido cancelado com sucesso!');
      setSelectedOrder(null);
      const activeRef = { active: true };
      loadOrders(activeRef);
    } catch (err: any) {
      alert(err.message || 'Erro ao cancelar pedido.');
    }
  }, [loadOrders]);

  if (loading) return <div className="loading">Carregando pedidos de compra...</div>;

  const steps = ['Pendente', 'Confirmado', 'Enviado', 'Entregue'];

  return (
    <div className="buyer-orders-view">
      <h2 className="section-title">Pedidos de Compra</h2>
      <div className="orders-list">
        {orders.length === 0 ? (
          <div className="glass-card empty-orders">Nenhum pedido de compra realizado ainda.</div>
        ) : (
          orders.map((order) => (
            <div key={order.id} className="order-item-row glass-card" onClick={() => setSelectedOrder(order)}>
              <div className="order-meta">
                <span className="order-id">ID: #{order.id.slice(0, 8)}</span>
                <span className="order-seller">Vendedor: {order.sellerCompanyName}</span>
              </div>
              <div className="order-values">
                <span className="order-total">R$ {order.total.toFixed(2)}</span>
                <span className={`status-badge ${order.status.toLowerCase()}`}>{order.status}</span>
              </div>
            </div>
          ))
        )}
      </div>

      {selectedOrder && (
        <div className="modal-overlay" onClick={() => setSelectedOrder(null)}>
          <div className="modal-content glass-card order-modal" onClick={(e) => e.stopPropagation()}>
            <h2>Detalhes do Pedido</h2>
            <p className="order-id-detail">ID Completo: {selectedOrder.id}</p>
            
            {/* Stepper Horizontal */}
            {selectedOrder.status !== 'Cancelado' ? (
              <div className="stepper-horizontal">
                {steps.map((step, idx) => {
                  const currentIdx = steps.indexOf(selectedOrder.status);
                  const isCompleted = idx <= currentIdx;
                  const isActive = idx === currentIdx;

                  return (
                    <div key={step} className={`step-wrapper ${isCompleted ? 'completed' : ''} ${isActive ? 'active' : ''}`}>
                      <div className="step-circle">{idx + 1}</div>
                      <span className="step-label">{step}</span>
                      {idx < steps.length - 1 && <div className="step-connector"></div>}
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="cancelled-alert">Este pedido foi Cancelado.</div>
            )}

            <div className="order-items-list-modal">
              <h4>Itens do Pedido:</h4>
              {selectedOrder.items?.map((item) => (
                <div key={item.id} className="modal-item-row">
                  <span>{item.productName} (x{item.qty})</span>
                  <span>R$ {(item.price * item.qty).toFixed(2)}</span>
                </div>
              ))}
            </div>

            {(selectedOrder.status === 'Pendente' || selectedOrder.status === 'Confirmado') && (
              <button className="btn-primary cancel-btn" onClick={() => handleCancelOrder(selectedOrder.id)}>
                Cancelar Pedido
              </button>
            )}

            <button className="btn-secondary close-modal" onClick={() => setSelectedOrder(null)}>Fechar</button>
          </div>
        </div>
      )}
    </div>
  );
};
