import React, { useEffect, useState, useCallback } from 'react';
import { productsService } from '../../services/products';
import type { Product } from '../../services/products';
import { useAuth } from '../../contexts/AuthContext';
import './ProductsView.css';

export const ProductsViewSeller: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [showFormModal, setShowFormModal] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Formulário
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [b2bMinQty, setB2bMinQty] = useState('');
  const [b2bPrice, setB2bPrice] = useState('');

  const { user } = useAuth();

  useEffect(() => {
    let active = true;

    const fetchProducts = async () => {
      try {
        const data = await productsService.listAll();
        if (!active) return;
        if (user) {
          setProducts(data.filter((p) => p.companyId === user.companyId));
        }
      } catch (err) {
        console.error('Erro ao buscar produtos:', err);
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    fetchProducts();

    return () => {
      active = false;
    };
  }, [user, user?.companyId, refreshTrigger]);

  const handleToggleActive = useCallback(async (product: Product) => {
    try {
      if (product.isActive) {
        await productsService.delete(product.id);
      } else {
        await productsService.update(product.id, {
          name: product.name,
          description: product.description,
          price: product.price,
          b2bMinQty: product.b2bMinQty,
          b2bPrice: product.b2bPrice,
          isActive: true,
        });
      }
      setRefreshTrigger((prev) => prev + 1);
    } catch (err: unknown) {
      const error = err as Error;
      alert(error.message || 'Erro ao alternar status do produto.');
    }
  }, []);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = {
        name,
        description,
        price: Number(price),
        b2bMinQty: b2bMinQty ? Number(b2bMinQty) : 0,
        b2bPrice: b2bPrice ? Number(b2bPrice) : Number(price),
        isActive: true,
      };
      await productsService.create(payload);
      setShowFormModal(false);
      // Reset
      setName('');
      setDescription('');
      setPrice('');
      setB2bMinQty('');
      setB2bPrice('');
      setRefreshTrigger((prev) => prev + 1);
    } catch (err: unknown) {
      const error = err as Error;
      alert(error.message || 'Erro ao cadastrar produto.');
    }
  }, [name, description, price, b2bMinQty, b2bPrice]);

  if (loading) return <div className="loading">Carregando catálogo de vendas...</div>;

  return (
    <div className="seller-products-view">
      <div className="seller-products-header">
        <h2 className="section-title">Gerenciar Meus Produtos</h2>
        <button className="btn-primary" onClick={() => setShowFormModal(true)}>Adicionar Produto</button>
      </div>

      <div className="table-responsive glass-card">
        <table className="seller-products-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Preço Unitário</th>
              <th>Mínimo Atacado B2B</th>
              <th>Preço B2B</th>
              <th>Visível Catalogo</th>
            </tr>
          </thead>
          <tbody>
            {products.length === 0 ? (
              <tr>
                <td colSpan={5} className="empty-table">Nenhum produto cadastrado por sua empresa.</td>
              </tr>
            ) : (
              products.map((product) => (
                <tr key={product.id}>
                  <td>
                    <strong>{product.name}</strong>
                    <p className="desc-td">{product.description || 'Sem descrição.'}</p>
                  </td>
                  <td>R$ {product.price.toFixed(2)}</td>
                  <td>{product.b2bMinQty > 0 ? `${product.b2bMinQty} un` : 'Inativo'}</td>
                  <td>{product.b2bMinQty > 0 ? `R$ ${product.b2bPrice.toFixed(2)}` : '-'}</td>
                  <td>
                    <label className="switch">
                      <input 
                        type="checkbox" 
                        checked={product.isActive} 
                        onChange={() => handleToggleActive(product)}
                      />
                      <span className="slider round"></span>
                    </label>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {showFormModal && (
        <div className="modal-overlay" onClick={() => setShowFormModal(false)}>
          <form className="modal-content glass-card form-modal" onClick={(e) => e.stopPropagation()} onSubmit={handleSubmit}>
            <h2>Novo Produto B2B</h2>
            
            <div className="form-group">
              <label>Nome do Produto</label>
              <input type="text" required value={name} onChange={(e) => setName(e.target.value)} placeholder="Ex: Cimento Portland 50kg" />
            </div>

            <div className="form-group">
              <label>Descrição</label>
              <textarea value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Especificações técnicas ou prazos..." />
            </div>

            <div className="form-group">
              <label>Preço Base (Unitário)</label>
              <input type="number" required step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="Ex: 35.00" />
            </div>

            <div className="form-group-row">
              <div className="form-group">
                <label>Mínimo de Lote (B2B)</label>
                <input type="number" value={b2bMinQty} onChange={(e) => setB2bMinQty(e.target.value)} placeholder="Ex: 50" />
              </div>
              <div className="form-group">
                <label>Preço de Atacado (B2B)</label>
                <input type="number" step="0.01" value={b2bPrice} onChange={(e) => setB2bPrice(e.target.value)} placeholder="Ex: 29.90" />
              </div>
            </div>

            <div className="modal-form-actions">
              <button type="button" className="btn-secondary" onClick={() => setShowFormModal(false)}>Cancelar</button>
              <button type="submit" className="btn-primary">Criar Produto</button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
