import React, { useEffect, useState, useCallback } from 'react';
import { ordersService } from '../../services/orders';
import type { Order } from '../../services/orders';
import './OrdersView.css';

export const OrdersViewSeller: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  useEffect(() => {
    let active = true;

    const fetchOrders = async () => {
      try {
        const data = await ordersService.list('seller');
        if (!active) return;
        setOrders(data);
      } catch (err) {
        console.error('Erro ao buscar pedidos recebidos:', err);
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    setLoading(true);
    fetchOrders();

    return () => {
      active = false;
    };
  }, [refreshTrigger]);

  const handleUpdateStatus = useCallback(async (orderId: string, currentStatus: string) => {
    let nextStatus = '';
    if (currentStatus === 'Pendente') nextStatus = 'Confirmado';
    else if (currentStatus === 'Confirmado') nextStatus = 'Enviado';
    else if (currentStatus === 'Enviado') nextStatus = 'Entregue';

    if (!nextStatus) return;

    try {
      await ordersService.updateStatus(orderId, nextStatus);
      alert(`Status do pedido atualizado para: ${nextStatus}`);
      setSelectedOrder(null);
      setRefreshTrigger((prev) => prev + 1);
    } catch (err: any) {
      alert(err.message || 'Erro ao transicionar pedido.');
    }
  }, []);

  if (loading) return <div className="loading">Carregando pedidos recebidos...</div>;

  return (
    <div className="seller-orders-view">
      <h2 className="section-title">Pedidos de Venda Recebidos</h2>
      <div className="orders-list">
        {orders.length === 0 ? (
          <div className="glass-card empty-orders">Nenhum pedido de venda recebido ainda.</div>
        ) : (
          orders.map((order) => (
            <div key={order.id} className="order-item-row glass-card" onClick={() => setSelectedOrder(order)}>
              <div className="order-meta">
                <span className="order-id">ID: #{order.id.slice(0, 8)}</span>
                <span className="order-seller">Cliente: {order.buyerCompanyName}</span>
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
            <h2>Gerenciar Venda</h2>
            <p className="order-id-detail">Comprador: {selectedOrder.buyerCompanyName}</p>
            
            <div className="order-items-list-modal">
              <h4>Itens Comprados:</h4>
              {selectedOrder.items?.map((item) => (
                <div key={item.id} className="modal-item-row">
                  <span>{item.productName} (x{item.qty})</span>
                  <span>R$ {(item.price * item.qty).toFixed(2)}</span>
                </div>
              ))}
              <div className="modal-item-row total-row-modal">
                <strong>Total da Venda:</strong>
                <strong>R$ {selectedOrder.total.toFixed(2)}</strong>
              </div>
            </div>

            {selectedOrder.status !== 'Cancelado' && selectedOrder.status !== 'Entregue' && (
              <div className="seller-action-block">
                <button 
                  className="btn-primary update-status-btn"
                  onClick={() => handleUpdateStatus(selectedOrder.id, selectedOrder.status)}
                >
                  {selectedOrder.status === 'Pendente' && 'Confirmar Recebimento (Confirmado)'}
                  {selectedOrder.status === 'Confirmado' && 'Despachar / Enviar Carga (Enviado)'}
                  {selectedOrder.status === 'Enviado' && 'Confirmar Entrega ao Cliente (Entregue)'}
                </button>
              </div>
            )}

            <button className="btn-secondary close-modal" onClick={() => setSelectedOrder(null)}>Fechar</button>
          </div>
        </div>
      )}
    </div>
  );
};
