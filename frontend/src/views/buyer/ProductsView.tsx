import React, { useEffect, useState } from 'react';
import { productsService } from '../../services/products';
import type { Product } from '../../services/products';
import { useCart } from '../../contexts/CartContext';
import './ProductsView.css';

export const ProductsView: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [qty, setQty] = useState(1);
  const { addToCart } = useCart();

  useEffect(() => {
    productsService.listAll()
      .then((data) => setProducts(data.filter((p) => p.isActive)))
      .finally(() => setLoading(false));
  }, []);

  const handleAddToCart = (product: Product) => {
    addToCart(product, qty);
    setSelectedProduct(null);
    setQty(1);
    alert('Produto adicionado ao carrinho!');
  };

  if (loading) return <div className="loading">Carregando catálogo corporativo...</div>;

  return (
    <div className="buyer-products-view">
      <h2 className="section-title">Catálogo Corporativo</h2>
      <div className="product-grid">
        {products.map((product) => (
          <div key={product.id} className="product-card glass-card" onClick={() => setSelectedProduct(product)}>
            <div className="card-header">
              <span className="seller-name">🏢 {product.companyName}</span>
            </div>
            <h3 className="product-name">{product.name}</h3>
            <p className="product-desc">{product.description || 'Sem descrição cadastrada.'}</p>
            <div className="price-matrix">
              <div className="price-row">
                <span className="price-label">Preço Normal:</span>
                <span className="price-value">R$ {product.price.toFixed(2)}</span>
              </div>
              {product.b2bMinQty > 0 && (
                <div className="price-row b2b-row">
                  <span className="price-label">Lote Atacado:</span>
                  <span className="price-value b2b-value">R$ {product.b2bPrice.toFixed(2)} (Min {product.b2bMinQty} un)</span>
                </div>
              )}
            </div>
            <button className="btn-primary card-add-btn">Ver Detalhes</button>
          </div>
        ))}
      </div>

      {selectedProduct && (
        <div className="modal-overlay" onClick={() => setSelectedProduct(null)}>
          <div className="modal-content glass-card" onClick={(e) => e.stopPropagation()}>
            <h2>{selectedProduct.name}</h2>
            <span className="seller-name-modal">Vendido por: {selectedProduct.companyName}</span>
            <p className="desc-modal">{selectedProduct.description || 'Este item B2B não possui descrição adicional.'}</p>
            
            <div className="modal-pricing">
              <div className="price-box">
                <span>Unitário Padrão</span>
                <strong>R$ {selectedProduct.price.toFixed(2)}</strong>
              </div>
              {selectedProduct.b2bMinQty > 0 && (
                <div className="price-box b2b">
                  <span>Atacado B2B (Min {selectedProduct.b2bMinQty} un)</span>
                  <strong>R$ {selectedProduct.b2bPrice.toFixed(2)}</strong>
                </div>
              )}
            </div>

            <div className="cart-add-row">
              <div className="qty-picker">
                <button onClick={() => setQty(Math.max(1, qty - 1))}>-</button>
                <input type="number" value={qty} onChange={(e) => setQty(Math.max(1, Number(e.target.value)))} />
                <button onClick={() => setQty(qty + 1)}>+</button>
              </div>
              <button className="btn-primary" onClick={() => handleAddToCart(selectedProduct)}>
                Adicionar ao Carrinho
              </button>
            </div>
            
            <button className="btn-secondary close-modal" onClick={() => setSelectedProduct(null)}>Fechar</button>
          </div>
        </div>
      )}
    </div>
  );
};
