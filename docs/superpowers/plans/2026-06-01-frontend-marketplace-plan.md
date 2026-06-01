# Plano de Implementação: Frontend E-commerce B2B

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir o frontend modular em React + Vite + TypeScript + Vanilla CSS para consumir o E-commerce B2B API, integrando login, alternador dinâmico de compras/vendas, catálogo B2B com descontos por quantidade e fluxo de pedidos corporativos.

**Architecture:** A aplicação será uma Single Page Application (SPA) modular localizada em `/frontend` na raiz do projeto. O gerenciamento de estado será feito via React Contexts (`AuthContext` e `CartContext`), a estilização em Vanilla CSS com HSL dinâmico, e o consumo de dados via fetch client com JWT interceptor e refresh token automático.

**Tech Stack:** React 19, Vite 6, TypeScript 5, Vanilla CSS.

---

### Task 1: Estruturação Inicial do Projeto (Scaffolding)

**Files:**
- Create: `frontend/` (projeto inteiro scaffolded)
- Verify: `frontend/package.json`, `frontend/vite.config.ts`, `frontend/tsconfig.json`

- [ ] **Passo 1: Criar o projeto React + Vite + TS utilizando modo não interativo**
  
  Execute na raiz `e:\E-commerce B2B API`:
  ```bash
  npx -y create-vite@latest frontend --template react-ts --no-interactive
  ```
  Esperado: A pasta `/frontend` deve ser criada com a estrutura básica React + TS do Vite.

- [ ] **Passo 2: Instalar as dependências necessárias de rotas e ícones**
  
  Execute na pasta `/frontend`:
  ```bash
  cd frontend
  npm install
  ```
  Esperado: Dependências de desenvolvimento e runtime instaladas com sucesso.

- [ ] **Passo 3: Verificar que o projeto executa localmente**
  
  Execute na pasta `/frontend`:
  ```bash
  npm run build
  ```
  Esperado: O comando `npm run build` deve compilar o projeto TypeScript sem erros de tipagem.

- [ ] **Passo 4: Commit das alterações**
  
  Execute:
  ```bash
  git add frontend/
  git commit -m "feat: estruturação inicial do projeto react + vite"
  ```

---

### Task 2: Sistema de Design e Estilos Globais (Vanilla CSS)

**Files:**
- Create: `frontend/src/styles/variables.css`
- Create: `frontend/src/styles/global.css`
- Modify: `frontend/src/main.tsx`

- [ ] **Passo 1: Criar o arquivo de variáveis de estilo**
  
  Crie o arquivo `frontend/src/styles/variables.css`:
  ```css
  :root {
    /* Cores Base Escuras */
    --background: 240 10% 3.9%;
    --foreground: 0 0% 98%;
    
    --card: 240 10% 5.9%;
    --card-foreground: 0 0% 98%;
    
    --border: 240 5.9% 15%;
    --input: 240 5.9% 10%;
    --input-focus: 262 83% 58%;
    
    /* Sotaques de Cores do Alternador */
    --accent-buyer: 262 83% 58%;        /* Violeta */
    --accent-buyer-hover: 262 83% 52%;
    
    --accent-seller: 142 71% 45%;       /* Esmeralda */
    --accent-seller-hover: 142 71% 39%;
    
    /* Dinâmicas */
    --primary: var(--accent-buyer);
    --primary-hover: var(--accent-buyer-hover);
    
    /* Status Pedidos */
    --status-pending: 38 92% 50%;
    --status-confirmed: 217 91% 60%;
    --status-shipped: 262 83% 58%;
    --status-delivered: 142 71% 45%;
    --status-cancelled: 0 84% 60%;
    
    /* Outros */
    --font-family: 'Outfit', 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
    --radius: 12px;
    --transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    --glass-bg: rgba(14, 14, 17, 0.75);
    --glass-border: rgba(255, 255, 255, 0.08);
    --shadow: 0 8px 32px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.05);
  }

  body.theme-buyer {
    --primary: var(--accent-buyer);
    --primary-hover: var(--accent-buyer-hover);
  }

  body.theme-seller {
    --primary: var(--accent-seller);
    --primary-hover: var(--accent-seller-hover);
  }
  ```

- [ ] **Passo 2: Criar o arquivo de estilos globais**
  
  Crie o arquivo `frontend/src/styles/global.css`:
  ```css
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=Outfit:wght@500;600;700&display=swap');

  * {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }

  body {
    background-color: hsl(var(--background));
    color: hsl(var(--foreground));
    font-family: var(--font-family);
    min-height: 100vh;
    overflow-x: hidden;
    transition: background-color 0.5s ease;
  }

  input, select, textarea {
    background-color: hsl(var(--input));
    color: hsl(var(--foreground));
    border: 1px solid hsl(var(--border));
    border-radius: var(--radius);
    padding: 10px 14px;
    font-family: inherit;
    font-size: 14px;
    outline: none;
    transition: var(--transition);
  }

  input:focus, select:focus, textarea:focus {
    border-color: hsl(var(--primary));
    box-shadow: 0 0 0 2px hsla(var(--primary), 0.2);
  }

  button {
    font-family: inherit;
    border-radius: var(--radius);
    padding: 10px 20px;
    font-weight: 600;
    cursor: pointer;
    transition: var(--transition);
    border: none;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }

  button.btn-primary {
    background-color: hsl(var(--primary));
    color: #fff;
  }

  button.btn-primary:hover {
    background-color: hsl(var(--primary-hover));
    transform: translateY(-1px);
  }

  button.btn-secondary {
    background-color: hsl(var(--card));
    border: 1px solid hsl(var(--border));
    color: hsl(var(--foreground));
  }

  button.btn-secondary:hover {
    background-color: hsla(var(--foreground), 0.05);
  }

  button:active {
    transform: scale(0.98);
  }

  .glass-card {
    background: var(--glass-bg);
    backdrop-filter: blur(12px);
    border: 1px solid var(--glass-border);
    box-shadow: var(--shadow);
    border-radius: var(--radius);
    padding: 24px;
  }
  ```

- [ ] **Passo 3: Importar os arquivos CSS globais**
  
  Modifique o `frontend/src/main.tsx` para incluir os arquivos CSS criados no topo e remover o `index.css` padrão:
  ```typescript
  // Substitua as importações de CSS padrão por:
  import './styles/variables.css';
  import './styles/global.css';
  ```

- [ ] **Passo 4: Commit das alterações**
  
  Execute:
  ```bash
  git add frontend/src/styles/ frontend/src/main.tsx
  git commit -m "feat: implementar design system e variaveis globais de CSS"
  ```

---

### Task 3: API Client com Rotação de JWT (`services/api.ts`)

**Files:**
- Create: `frontend/src/services/api.ts`

- [ ] **Passo 1: Criar o cliente de API baseado em Fetch**
  
  Crie o arquivo `frontend/src/services/api.ts` para servir como middleware entre o React e o servidor local de desenvolvimento na porta `http://localhost:5012`:
  ```typescript
  const API_BASE_URL = 'http://localhost:5012';

  interface RequestOptions extends RequestInit {
    params?: Record<string, string>;
  }

  export class ApiError extends Error {
    constructor(public status: number, message: string, public body?: any) {
      super(message);
    }
  }

  async function refreshAccessToken(): Promise<string | null> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) return null;

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        return null;
      }

      const data = await response.json();
      localStorage.setItem('access_token', data.accessToken);
      localStorage.setItem('refresh_token', data.refreshToken);
      return data.accessToken;
    } catch {
      return null;
    }
  }

  export async function apiRequest<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    let token = localStorage.getItem('access_token');
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    let url = `${API_BASE_URL}${endpoint}`;
    if (options.params) {
      const searchParams = new URLSearchParams(options.params);
      url += `?${searchParams.toString()}`;
    }

    let response = await fetch(url, {
      ...options,
      headers,
    });

    if (response.status === 401) {
      // Tentativa de rotação de refresh token
      const newToken = await refreshAccessToken();
      if (newToken) {
        headers['Authorization'] = `Bearer ${newToken}`;
        response = await fetch(url, {
          ...options,
          headers,
        });
      } else {
        window.dispatchEvent(new Event('auth-expired'));
        throw new ApiError(401, 'Sessão expirada. Faça login novamente.');
      }
    }

    if (!response.ok) {
      let body: any;
      try {
        body = await response.json();
      } catch {
        body = null;
      }
      throw new ApiError(response.status, body?.message || 'Erro na requisição à API', body);
    }

    if (response.status === 204) {
      return {} as T;
    }

    return response.json() as Promise<T>;
  }
  ```

- [ ] **Passo 2: Commit do cliente de API**
  
  Execute:
  ```bash
  git add frontend/src/services/api.ts
  git commit -m "feat: adicionar cliente de api com suporte a JWT e auto-refresh"
  ```

---

### Task 4: Serviço e Contexto de Autenticação (`AuthContext`)

**Files:**
- Create: `frontend/src/services/auth.ts`
- Create: `frontend/src/contexts/AuthContext.tsx`

- [ ] **Passo 1: Criar endpoints do serviço de autenticação**
  
  Crie o arquivo `frontend/src/services/auth.ts`:
  ```typescript
  import { apiRequest } from './api';

  export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
  }

  export interface UserProfile {
    id: string;
    name: string;
    email: string;
    companyId: string;
    companyName: string;
  }

  export const authService = {
    login: async (email: string, password: string): Promise<LoginResponse> => {
      return apiRequest<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      });
    },

    register: async (payload: any): Promise<LoginResponse> => {
      return apiRequest<LoginResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(payload),
      });
    },

    getProfile: async (companyId: string, userId: string): Promise<UserProfile> => {
      return apiRequest<UserProfile>(`/api/companies/${companyId}/users/${userId}`);
    },
  };
  ```

- [ ] **Passo 2: Criar o AuthContext**
  
  Crie o arquivo `frontend/src/contexts/AuthContext.tsx` para persistir dados do usuário e injetar classe correspondente no Body:
  ```typescript
  import React, { createContext, useState, useEffect, useContext } from 'react';
  import { LoginResponse, UserProfile, authService } from '../services/auth';

  interface AuthContextType {
    isAuthenticated: boolean;
    user: UserProfile | null;
    mode: 'buyer' | 'seller';
    login: (email: string, password: string) => Promise<void>;
    register: (payload: any) => Promise<void>;
    logout: () => void;
    toggleMode: () => void;
    loading: boolean;
  }

  const AuthContext = createContext<AuthContextType | undefined>(undefined);

  export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<UserProfile | null>(null);
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(!!localStorage.getItem('access_token'));
    const [mode, setMode] = useState<'buyer' | 'seller'>('buyer');
    const [loading, setLoading] = useState<boolean>(true);

    const parseJwt = (token: string) => {
      try {
        return JSON.parse(atob(token.split('.')[1]));
      } catch {
        return null;
      }
    };

    const loadUserFromToken = () => {
      const token = localStorage.getItem('access_token');
      if (token) {
        const decoded = parseJwt(token);
        if (decoded) {
          setUser({
            id: decoded.sub,
            name: decoded.name || 'Administrador',
            email: decoded.email || '',
            companyId: decoded.company_id,
            companyName: decoded.company_name || 'Minha Empresa B2B',
          });
          setIsAuthenticated(true);
        }
      }
      setLoading(false);
    };

    useEffect(() => {
      loadUserFromToken();
      
      const handleExpired = () => logout();
      window.addEventListener('auth-expired', handleExpired);
      return () => window.removeEventListener('auth-expired', handleExpired);
    }, []);

    useEffect(() => {
      document.body.className = mode === 'buyer' ? 'theme-buyer' : 'theme-seller';
    }, [mode]);

    const login = async (email: string, password: string) => {
      const response = await authService.login(email, password);
      localStorage.setItem('access_token', response.accessToken);
      localStorage.setItem('refresh_token', response.refreshToken);
      setIsAuthenticated(true);
      loadUserFromToken();
    };

    const register = async (payload: any) => {
      const response = await authService.register(payload);
      localStorage.setItem('access_token', response.accessToken);
      localStorage.setItem('refresh_token', response.refreshToken);
      setIsAuthenticated(true);
      loadUserFromToken();
    };

    const logout = () => {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      setUser(null);
      setIsAuthenticated(false);
    };

    const toggleMode = () => {
      setMode((prev) => (prev === 'buyer' ? 'seller' : 'buyer'));
    };

    return (
      <AuthContext.Provider value={{ isAuthenticated, user, mode, login, register, logout, toggleMode, loading }}>
        {children}
      </AuthContext.Provider>
    );
  };

  export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth deve ser usado dentro de um AuthProvider');
    return context;
  };
  ```

- [ ] **Passo 3: Commit do Contexto de Auth**
  
  Execute:
  ```bash
  git add frontend/src/services/auth.ts frontend/src/contexts/AuthContext.tsx
  git commit -m "feat: criar servico e provedor de autenticacao com alternador de temas"
  ```

---

### Task 5: Contexto do Carrinho Inteligente B2B (`CartContext`)

**Files:**
- Create: `frontend/src/contexts/CartContext.tsx`

- [ ] **Passo 1: Criar o CartContext com as regras de validação B2B**
  
  Crie o arquivo `frontend/src/contexts/CartContext.tsx`:
  ```typescript
  import React, { createContext, useContext, useState, useEffect } from 'react';
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
    addToCart: (product: any, qty: number) => void;
    removeFromCart: (productId: string) => void;
    updateQty: (productId: string, qty: number) => void;
    clearCart: () => void;
    cartTotal: number;
    cartOriginalTotal: number;
    cartSavings: number;
  }

  const CartContext = createContext<CartContextType | undefined>(undefined);

  export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [items, setItems] = useState<CartItem[]>([]);
    const { user } = useAuth();

    // Reset carrinho ao trocar de conta
    useEffect(() => {
      setItems([]);
    }, [user?.id]);

    const addToCart = (product: any, qty: number) => {
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
    };

    const removeFromCart = (productId: string) => {
      setItems((prev) => prev.filter((item) => item.id !== productId));
    };

    const updateQty = (productId: string, qty: number) => {
      if (qty <= 0) {
        removeFromCart(productId);
        return;
      }
      setItems((prev) =>
        prev.map((item) => (item.id === productId ? { ...item, qty } : item))
      );
    };

    const clearCart = () => setItems([]);

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
  ```

- [ ] **Passo 2: Commit do Contexto do Carrinho**
  
  Execute:
  ```bash
  git add frontend/src/contexts/CartContext.tsx
  git commit -m "feat: criar CartContext com calculo dinamico de precos em lote B2B"
  ```

---

### Task 6: Cabeçalho com Alternador Deslizante (`Header.tsx`)

**Files:**
- Create: `frontend/src/components/layout/Header.tsx`
- Create: `frontend/src/components/layout/Header.css`

- [ ] **Passo 1: Criar o código estilizado do Header**
  
  Crie o arquivo `frontend/src/components/layout/Header.tsx`:
  ```typescript
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
  ```

- [ ] **Passo 2: Criar a estilização dedicada do Header**
  
  Crie o arquivo `frontend/src/components/layout/Header.css`:
  ```css
  .main-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 32px;
    border-radius: 0 0 var(--radius) var(--radius);
    position: sticky;
    top: 0;
    z-index: 100;
    margin-bottom: 24px;
    border-top: none;
  }

  .header-brand {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .logo-icon {
    font-size: 24px;
  }

  .logo-text {
    font-size: 20px;
    font-weight: 700;
    font-family: 'Outfit', sans-serif;
    letter-spacing: -0.5px;
    background: linear-gradient(135deg, hsl(var(--primary)), #00ffcc);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }

  .header-tenant-info .tenant-badge {
    background-color: hsla(var(--primary), 0.1);
    color: hsl(var(--primary));
    border: 1px solid hsla(var(--primary), 0.2);
    border-radius: 20px;
    padding: 6px 14px;
    font-size: 13px;
    font-weight: 500;
  }

  .header-actions {
    display: flex;
    align-items: center;
    gap: 20px;
  }

  .mode-toggle-container {
    background-color: hsl(var(--input));
    border: 1px solid hsl(var(--border));
    border-radius: 30px;
    display: flex;
    padding: 3px;
    position: relative;
    width: 180px;
    height: 38px;
  }

  .mode-toggle-btn {
    flex: 1;
    background: none;
    border: none;
    color: hsla(var(--foreground), 0.6);
    font-size: 13px;
    font-weight: 600;
    border-radius: 25px;
    transition: var(--transition);
    padding: 0;
  }

  .mode-toggle-btn.active {
    background-color: hsl(var(--primary));
    color: #fff;
    box-shadow: 0 4px 10px hsla(var(--primary), 0.3);
  }

  .cart-badge-wrapper {
    position: relative;
    font-size: 20px;
    cursor: pointer;
  }

  .cart-count {
    position: absolute;
    top: -8px;
    right: -10px;
    background-color: hsl(var(--accent-buyer));
    color: #fff;
    border-radius: 50%;
    font-size: 10px;
    width: 16px;
    height: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
  }

  .user-profile-wrapper {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .user-avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background-color: hsla(var(--primary), 0.2);
    color: hsl(var(--primary));
    border: 1px solid hsl(var(--primary));
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
    font-size: 14px;
  }

  .logout-btn {
    background: none;
    border: none;
    color: hsla(var(--foreground), 0.5);
    padding: 4px 8px;
    font-size: 13px;
  }

  .logout-btn:hover {
    color: hsl(var(--status-cancelled));
  }
  ```

- [ ] **Passo 3: Commit do Header**
  
  Execute:
  ```bash
  git add frontend/src/components/layout/
  git commit -m "feat: criar componente Header com alternador compras/vendas deslizante"
  ```

---

### Task 7: Vistas e Fluxos do Comprador (Compras)

**Files:**
- Create: `frontend/src/services/products.ts`
- Create: `frontend/src/services/orders.ts`
- Create: `frontend/src/views/buyer/ProductsView.tsx`
- Create: `frontend/src/views/buyer/CartView.tsx`
- Create: `frontend/src/views/buyer/OrdersView.tsx`

- [ ] **Passo 1: Criar endpoints das APIs de Catalogo de Produtos e Pedidos**
  
  Crie o arquivo `frontend/src/services/products.ts`:
  ```typescript
  import { apiRequest } from './api';

  export interface Product {
    id: string;
    name: string;
    description?: string;
    price: number;
    b2bMinQty: number;
    b2bPrice: number;
    companyId: string;
    companyName: string;
    isActive: boolean;
  }

  export const productsService = {
    listAll: async (params?: Record<string, string>): Promise<Product[]> => {
      const res = await apiRequest<{ items: Product[] }>('/api/products', { params });
      return res.items || [];
    },
    
    getById: async (id: string): Promise<Product> => {
      return apiRequest<Product>(`/api/products/${id}`);
    },

    create: async (payload: any): Promise<Product> => {
      return apiRequest<Product>('/api/products', {
        method: 'POST',
        body: JSON.stringify(payload),
      });
    },

    update: async (id: string, payload: any): Promise<Product> => {
      return apiRequest<Product>(`/api/products/${id}`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      });
    },

    delete: async (id: string): Promise<void> => {
      return apiRequest<void>(`/api/products/${id}`, { method: 'DELETE' });
    },
  };
  ```

  Crie o arquivo `frontend/src/services/orders.ts`:
  ```typescript
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
  ```

- [ ] **Passo 2: Criar a Tela de Catálogo de Produtos do Comprador**
  
  Crie o arquivo `frontend/src/views/buyer/ProductsView.tsx`:
  ```typescript
  import React, { useEffect, useState } from 'react';
  import { productsService, Product } from '../../services/products';
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
  ```

  Crie o arquivo `frontend/src/views/buyer/ProductsView.css` para a estilização:
  ```css
  .buyer-products-view {
    padding: 0 24px 40px 24px;
    max-width: 1200px;
    margin: 0 auto;
  }

  .section-title {
    font-size: 24px;
    font-weight: 700;
    margin-bottom: 24px;
    font-family: 'Outfit', sans-serif;
  }

  .product-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 24px;
  }

  .product-card {
    cursor: pointer;
    display: flex;
    flex-direction: column;
    height: 100%;
    transition: var(--transition);
  }

  .product-card:hover {
    transform: translateY(-4px);
    border-color: hsla(var(--primary), 0.4);
    box-shadow: 0 12px 40px hsla(var(--primary), 0.2);
  }

  .seller-name {
    font-size: 12px;
    color: hsla(var(--foreground), 0.5);
    font-weight: 500;
  }

  .product-name {
    font-family: 'Outfit', sans-serif;
    font-size: 18px;
    margin: 12px 0 8px 0;
  }

  .product-desc {
    font-size: 13px;
    color: hsla(var(--foreground), 0.6);
    line-height: 1.5;
    flex-grow: 1;
    margin-bottom: 16px;
  }

  .price-matrix {
    border-top: 1px solid hsl(var(--border));
    padding-top: 12px;
    margin-bottom: 16px;
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .price-row {
    display: flex;
    justify-content: space-between;
    font-size: 13px;
  }

  .price-value {
    font-weight: 600;
  }

  .b2b-row {
    color: hsl(var(--accent-seller));
    font-weight: 500;
  }

  .card-add-btn {
    width: 100%;
  }

  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-content {
    max-width: 500px;
    width: 90%;
    position: relative;
  }

  .seller-name-modal {
    font-size: 13px;
    color: hsla(var(--foreground), 0.5);
  }

  .desc-modal {
    margin: 16px 0;
    line-height: 1.6;
    color: hsla(var(--foreground), 0.8);
  }

  .modal-pricing {
    display: flex;
    gap: 16px;
    margin-bottom: 24px;
  }

  .price-box {
    flex: 1;
    background-color: hsl(var(--input));
    border: 1px solid hsl(var(--border));
    border-radius: var(--radius);
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .price-box span {
    font-size: 11px;
    color: hsla(var(--foreground), 0.5);
  }

  .price-box.b2b {
    border-color: hsla(var(--accent-seller), 0.3);
    color: hsl(var(--accent-seller));
  }

  .cart-add-row {
    display: flex;
    gap: 16px;
    margin-bottom: 16px;
  }

  .qty-picker {
    display: inline-flex;
    border: 1px solid hsl(var(--border));
    border-radius: var(--radius);
    overflow: hidden;
  }

  .qty-picker button {
    border-radius: 0;
    padding: 0 14px;
    background-color: hsl(var(--input));
  }

  .qty-picker input {
    border: none;
    border-radius: 0;
    width: 60px;
    text-align: center;
    background: none;
  }

  .close-modal {
    width: 100%;
  }
  ```

- [ ] **Passo 3: Criar a Tela do Carrinho Inteligente**
  
  Crie o arquivo `frontend/src/views/buyer/CartView.tsx`:
  ```typescript
  import React from 'react';
  import { useCart } from '../../contexts/CartContext';
  import { ordersService } from '../../services/orders';
  import './CartView.css';

  export const CartView: React.FC = () => {
    const { items, updateQty, removeFromCart, cartTotal, cartOriginalTotal, cartSavings, clearCart } = useCart();

    const handleCheckout = async () => {
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
    };

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
  ```

  Crie o arquivo `frontend/src/views/buyer/CartView.css`:
  ```css
  .cart-view {
    padding: 0 24px 40px 24px;
    max-width: 1200px;
    margin: 0 auto;
  }

  .cart-view-empty {
    text-align: center;
    padding: 60px 40px;
    max-width: 600px;
    margin: 40px auto;
  }

  .cart-view-empty p {
    color: hsla(var(--foreground), 0.6);
    margin-top: 12px;
  }

  .cart-content {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 32px;
  }

  .cart-items-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .cart-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 20px;
  }

  .item-unit-price {
    margin-top: 8px;
    font-size: 14px;
  }

  .old-price {
    text-decoration: line-through;
    color: hsla(var(--foreground), 0.4);
    margin-right: 8px;
  }

  .active-b2b-price {
    color: hsl(var(--accent-seller));
    font-weight: 600;
  }

  .item-actions {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 10px;
  }

  .b2b-alert {
    font-size: 11px;
    color: hsl(var(--status-pending));
    background-color: hsla(var(--status-pending), 0.1);
    padding: 4px 8px;
    border-radius: 4px;
  }

  .remove-item-btn {
    background: none;
    border: none;
    color: hsla(var(--foreground), 0.4);
    font-size: 12px;
    cursor: pointer;
  }

  .remove-item-btn:hover {
    color: hsl(var(--status-cancelled));
  }

  .cart-summary {
    height: fit-content;
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .summary-row {
    display: flex;
    justify-content: space-between;
    font-size: 14px;
  }

  .summary-row.savings {
    color: hsl(var(--accent-seller));
    font-weight: 600;
  }

  .summary-row.total {
    border-top: 1px solid hsl(var(--border));
    padding-top: 16px;
    font-size: 18px;
    font-weight: 700;
  }

  .checkout-btn {
    width: 100%;
    margin-top: 12px;
  }
  ```

- [ ] **Passo 4: Criar a Tela de Meus Pedidos do Comprador com Stepper Horizontal**
  
  Crie o arquivo `frontend/src/views/buyer/OrdersView.tsx`:
  ```typescript
  import React, { useEffect, useState } from 'react';
  import { ordersService, Order } from '../../services/orders';
  import './OrdersView.css';

  export const OrdersView: React.FC = () => {
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

    const loadOrders = () => {
      ordersService.list('buyer')
        .then((data) => setOrders(data))
        .finally(() => setLoading(false));
    };

    useEffect(() => {
      loadOrders();
    }, []);

    const handleCancelOrder = async (orderId: string) => {
      if (!confirm('Deseja realmente cancelar este pedido?')) return;
      try {
        await ordersService.updateStatus(orderId, 'Cancelado');
        alert('Pedido cancelado com sucesso!');
        setSelectedOrder(null);
        loadOrders();
      } catch (err: any) {
        alert(err.message || 'Erro ao cancelar pedido.');
      }
    };

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
  ```

  Crie o arquivo `frontend/src/views/buyer/OrdersView.css`:
  ```css
  .buyer-orders-view {
    padding: 0 24px 40px 24px;
    max-width: 1000px;
    margin: 0 auto;
  }

  .orders-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .order-item-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 24px;
    cursor: pointer;
    transition: var(--transition);
  }

  .order-item-row:hover {
    transform: translateX(4px);
    border-color: hsla(var(--primary), 0.3);
  }

  .order-meta {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .order-id {
    font-weight: 700;
    font-size: 14px;
  }

  .order-seller {
    font-size: 13px;
    color: hsla(var(--foreground), 0.5);
  }

  .order-values {
    display: flex;
    align-items: center;
    gap: 20px;
  }

  .order-total {
    font-weight: 700;
    font-size: 16px;
  }

  .status-badge {
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
  }

  .status-badge.pendente { background-color: hsla(var(--status-pending), 0.15); color: hsl(var(--status-pending)); }
  .status-badge.confirmado { background-color: hsla(var(--status-confirmed), 0.15); color: hsl(var(--status-confirmed)); }
  .status-badge.enviado { background-color: hsla(var(--status-shipped), 0.15); color: hsl(var(--status-shipped)); }
  .status-badge.entregue { background-color: hsla(var(--status-delivered), 0.15); color: hsl(var(--status-delivered)); }
  .status-badge.cancelado { background-color: hsla(var(--status-cancelled), 0.15); color: hsl(var(--status-cancelled)); }

  .order-modal {
    max-width: 600px !important;
  }

  .order-id-detail {
    font-size: 12px;
    color: hsla(var(--foreground), 0.4);
    margin-bottom: 24px;
  }

  .stepper-horizontal {
    display: flex;
    justify-content: space-between;
    position: relative;
    margin-bottom: 32px;
    padding: 0 10px;
  }

  .step-wrapper {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex: 1;
    position: relative;
    z-index: 1;
  }

  .step-circle {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background-color: hsl(var(--input));
    border: 2px solid hsl(var(--border));
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
    font-size: 13px;
    color: hsla(var(--foreground), 0.4);
    transition: var(--transition);
  }

  .step-label {
    font-size: 11px;
    font-weight: 500;
    margin-top: 8px;
    color: hsla(var(--foreground), 0.4);
  }

  .step-wrapper.completed .step-circle {
    background-color: hsl(var(--primary));
    border-color: hsl(var(--primary));
    color: #fff;
  }

  .step-wrapper.completed .step-label {
    color: hsl(var(--foreground));
  }

  .step-wrapper.active .step-circle {
    box-shadow: 0 0 0 4px hsla(var(--primary), 0.25);
  }

  .step-connector {
    position: absolute;
    top: 15px;
    left: calc(50% + 16px);
    right: calc(-50% + 16px);
    height: 2px;
    background-color: hsl(var(--border));
    z-index: -1;
  }

  .step-wrapper.completed .step-connector {
    background-color: hsl(var(--primary));
  }

  .cancelled-alert {
    background-color: hsla(var(--status-cancelled), 0.15);
    color: hsl(var(--status-cancelled));
    border: 1px solid hsla(var(--status-cancelled), 0.3);
    padding: 12px;
    border-radius: var(--radius);
    text-align: center;
    margin-bottom: 24px;
    font-weight: 600;
  }

  .order-items-list-modal {
    border-top: 1px solid hsl(var(--border));
    padding-top: 16px;
    margin-bottom: 24px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .modal-item-row {
    display: flex;
    justify-content: space-between;
    font-size: 14px;
  }

  .cancel-btn {
    background-color: hsl(var(--status-cancelled)) !important;
    width: 100%;
    margin-bottom: 12px;
  }
  ```

- [ ] **Passo 5: Commit dos arquivos de compras**
  
  Execute:
  ```bash
  git add frontend/src/services/products.ts frontend/src/services/orders.ts frontend/src/views/buyer/
  git commit -m "feat: criar servicos e views de compras do comprador"
  ```

---

### Task 8: Vistas e Fluxos do Vendedor (Vendas)

**Files:**
- Create: `frontend/src/views/seller/ProductsView.tsx`
- Create: `frontend/src/views/seller/OrdersView.tsx`

- [ ] **Passo 1: Criar a Tela de Catálogo de Vendas do Vendedor (CRUD + Soft Delete)**
  
  Crie o arquivo `frontend/src/views/seller/ProductsView.tsx`:
  ```typescript
  import React, { useEffect, useState } from 'react';
  import { productsService, Product } from '../../services/products';
  import { useAuth } from '../../contexts/AuthContext';
  import './ProductsView.css';

  export const ProductsViewSeller: React.FC = () => {
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const [showFormModal, setShowFormModal] = useState(false);
    
    // Formulário
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');
    const [price, setPrice] = useState('');
    const [b2bMinQty, setB2bMinQty] = useState('');
    const [b2bPrice, setB2bPrice] = useState('');
    
    const { user } = useAuth();

    const loadProducts = () => {
      productsService.listAll()
        .then((data) => {
          // Filtrar apenas produtos da minha empresa (vendedor)
          if (user) {
            setProducts(data.filter((p) => p.companyId === user.companyId));
          }
        })
        .finally(() => setLoading(false));
    };

    useEffect(() => {
      loadProducts();
    }, [user?.companyId]);

    const handleToggleActive = async (product: Product) => {
      try {
        if (product.isActive) {
          await productsService.delete(product.id);
        } else {
          // Reativar via PUT com isActive = true
          await productsService.update(product.id, {
            name: product.name,
            description: product.description,
            price: product.price,
            b2bMinQty: product.b2bMinQty,
            b2bPrice: product.b2bPrice,
            isActive: true,
          });
        }
        loadProducts();
      } catch (err: any) {
        alert(err.message || 'Erro ao alternar status do produto.');
      }
    };

    const handleSubmit = async (e: React.FormEvent) => {
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
        loadProducts();
      } catch (err: any) {
        alert(err.message || 'Erro ao cadastrar produto.');
      }
    };

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
  ```

  Crie o arquivo `frontend/src/views/seller/ProductsView.css` para a estilização do CRUD de vendas:
  ```css
  .seller-products-view {
    padding: 0 24px 40px 24px;
    max-width: 1200px;
    margin: 0 auto;
  }

  .seller-products-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
  }

  .table-responsive {
    padding: 0;
    overflow-x: auto;
  }

  .seller-products-table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
    font-size: 14px;
  }

  .seller-products-table th, .seller-products-table td {
    padding: 16px 24px;
    border-bottom: 1px solid hsl(var(--border));
  }

  .seller-products-table th {
    background-color: hsla(var(--foreground), 0.02);
    font-weight: 600;
    color: hsla(var(--foreground), 0.6);
  }

  .desc-td {
    font-size: 11px;
    color: hsla(var(--foreground), 0.5);
    margin-top: 4px;
  }

  .empty-table {
    text-align: center;
    color: hsla(var(--foreground), 0.5);
    padding: 40px !important;
  }

  /* Form modal */
  .form-modal {
    max-width: 500px;
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .form-group label {
    font-size: 13px;
    font-weight: 500;
    color: hsla(var(--foreground), 0.8);
  }

  .form-group-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
  }

  .modal-form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 16px;
    margin-top: 12px;
  }

  /* Toggle Switch */
  .switch {
    position: relative;
    display: inline-block;
    width: 44px;
    height: 22px;
  }

  .switch input { 
    opacity: 0;
    width: 0;
    height: 0;
  }

  .slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: hsl(var(--input));
    border: 1px solid hsl(var(--border));
    transition: .4s;
  }

  .slider:before {
    position: absolute;
    content: "";
    height: 14px;
    width: 14px;
    left: 3px;
    bottom: 3px;
    background-color: hsla(var(--foreground), 0.4);
    transition: .4s;
  }

  input:checked + .slider {
    background-color: hsl(var(--accent-seller));
    border-color: hsl(var(--accent-seller));
  }

  input:checked + .slider:before {
    transform: translateX(22px);
    background-color: white;
  }

  .slider.round {
    border-radius: 34px;
  }

  .slider.round:before {
    border-radius: 50%;
  }
  ```

- [ ] **Passo 2: Criar a Tela de Gestão de Pedidos de Venda do Vendedor**
  
  Crie o arquivo `frontend/src/views/seller/OrdersView.tsx` para mudar status na máquina de estados:
  ```typescript
  import React, { useEffect, useState } from 'react';
  import { ordersService, Order } from '../../services/orders';
  import './OrdersView.css';

  export const OrdersViewSeller: React.FC = () => {
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

    const loadOrders = () => {
      ordersService.list('seller')
        .then((data) => setOrders(data))
        .finally(() => setLoading(false));
    };

    useEffect(() => {
      loadOrders();
    }, []);

    const handleUpdateStatus = async (orderId: string, currentStatus: string) => {
      let nextStatus = '';
      if (currentStatus === 'Pendente') nextStatus = 'Confirmado';
      else if (currentStatus === 'Confirmado') nextStatus = 'Enviado';
      else if (currentStatus === 'Enviado') nextStatus = 'Entregue';

      if (!nextStatus) return;

      try {
        await ordersService.updateStatus(orderId, nextStatus);
        alert(`Status do pedido atualizado para: ${nextStatus}`);
        setSelectedOrder(null);
        loadOrders();
      } catch (err: any) {
        alert(err.message || 'Erro ao transicionar pedido.');
      }
    };

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
  ```

  Crie o arquivo `frontend/src/views/seller/OrdersView.css`:
  ```css
  .seller-orders-view {
    padding: 0 24px 40px 24px;
    max-width: 1000px;
    margin: 0 auto;
  }

  .seller-action-block {
    margin-bottom: 16px;
  }

  .update-status-btn {
    width: 100%;
    background-color: hsl(var(--accent-seller)) !important;
  }

  .update-status-btn:hover {
    background-color: hsl(var(--accent-seller-hover)) !important;
  }

  .total-row-modal {
    border-top: 1px solid hsl(var(--border));
    padding-top: 12px;
    font-size: 16px;
    margin-top: 6px;
  }
  ```

- [ ] **Passo 3: Commit dos arquivos de vendas**
  
  Execute:
  ```bash
  git add frontend/src/views/seller/
  git commit -m "feat: criar views de gerenciamento de produtos e vendas do vendedor"
  ```

---

### Task 9: Fluxo de Autenticação e Integração de Rotas (`App.tsx`)

**Files:**
- Create: `frontend/src/views/auth/AuthView.tsx`
- Create: `frontend/src/views/auth/AuthView.css`
- Modify: `frontend/src/App.tsx`

- [ ] **Passo 1: Criar a Tela Unificada de Login/Cadastro Corporativo**
  
  Crie o arquivo `frontend/src/views/auth/AuthView.tsx`:
  ```typescript
  import React, { useState } from 'react';
  import { useAuth } from '../../contexts/AuthContext';
  import './AuthView.css';

  export const AuthView: React.FC = () => {
    const [isLogin, setIsLogin] = useState(true);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    
    // Cadastro
    const [companyName, setCompanyName] = useState('');
    const [adminName, setAdminName] = useState('');
    
    const { login, register } = useAuth();
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
      e.preventDefault();
      setLoading(true);
      try {
        if (isLogin) {
          await login(email, password);
        } else {
          await register({
            companyName,
            adminName,
            email,
            password,
          });
        }
      } catch (err: any) {
        alert(err.message || 'Erro ao autenticar. Verifique os dados.');
      } finally {
        setLoading(false);
      }
    };

    return (
      <div className="auth-view-overlay">
        <div className="auth-card glass-card">
          <div className="auth-header">
            🏢 <h2>B2B Marketplace Portal</h2>
            <p>Gerencie compras e suprimentos corporativos em escala</p>
          </div>

          <form onSubmit={handleSubmit} className="auth-form">
            {!isLogin && (
              <>
                <div className="form-group">
                  <label>Razão Social da Empresa</label>
                  <input type="text" required value={companyName} onChange={(e) => setCompanyName(e.target.value)} placeholder="Ex: Construtora Alvorada Ltda" />
                </div>
                <div className="form-group">
                  <label>Nome Completo do Administrador</label>
                  <input type="text" required value={adminName} onChange={(e) => setAdminName(e.target.value)} placeholder="Ex: João da Silva" />
                </div>
              </>
            )}

            <div className="form-group">
              <label>E-mail Corporativo</label>
              <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} placeholder="admin@empresa.com" />
            </div>

            <div className="form-group">
              <label>Senha de Segurança</label>
              <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Senha (min 6 caracteres)" />
            </div>

            <button type="submit" className="btn-primary auth-submit-btn" disabled={loading}>
              {loading ? 'Processando...' : isLogin ? 'Acessar Conta B2B' : 'Cadastrar Empresa'}
            </button>
          </form>

          <div className="auth-switcher">
            <button className="switch-btn" onClick={() => setIsLogin(!isLogin)}>
              {isLogin ? 'Nova empresa? Cadastre-se aqui' : 'Já possui conta? Faça o login corporativo'}
            </button>
          </div>
        </div>
      </div>
    );
  };
  ```

  Crie o arquivo `frontend/src/views/auth/AuthView.css`:
  ```css
  .auth-view-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: radial-gradient(circle at 10% 20%, rgba(124, 58, 237, 0.15) 0%, rgba(0, 0, 0, 0) 50%),
                radial-gradient(circle at 90% 80%, rgba(16, 185, 129, 0.15) 0%, rgba(0, 0, 0, 0) 50%),
                hsl(var(--background));
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10000;
  }

  .auth-card {
    max-width: 440px;
    width: 90%;
    display: flex;
    flex-direction: column;
    gap: 24px;
    padding: 40px !important;
  }

  .auth-header {
    text-align: center;
  }

  .auth-header h2 {
    font-family: 'Outfit', sans-serif;
    font-size: 22px;
    margin-top: 8px;
  }

  .auth-header p {
    font-size: 13px;
    color: hsla(var(--foreground), 0.5);
    margin-top: 6px;
    line-height: 1.4;
  }

  .auth-form {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .auth-submit-btn {
    width: 100%;
    margin-top: 8px;
    padding: 12px 20px;
  }

  .auth-switcher {
    text-align: center;
    border-top: 1px solid hsl(var(--border));
    padding-top: 16px;
  }

  .switch-btn {
    background: none;
    border: none;
    color: hsl(var(--primary));
    font-size: 13px;
    font-weight: 500;
  }

  .switch-btn:hover {
    text-decoration: underline;
  }
  ```

- [ ] **Passo 2: Integrar todas as Views e os Provedores Globais no App.tsx**
  
  Modifique o `frontend/src/App.tsx` para coordenar toda a roteamento interno e renderização com abas:
  ```typescript
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
  ```

  Crie o arquivo `frontend/src/App.css` para a estilização principal da integração das abas e contêiner:
  ```css
  .app-container {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .app-loading {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    font-size: 18px;
    font-weight: 600;
  }

  .sub-navigation {
    display: flex;
    justify-content: center;
    gap: 16px;
    margin-bottom: 32px;
  }

  .sub-nav-btn {
    background: none;
    border: 1px solid transparent;
    color: hsla(var(--foreground), 0.5);
    border-radius: 20px;
    padding: 8px 20px;
    font-size: 14px;
    transition: var(--transition);
  }

  .sub-nav-btn:hover {
    color: hsl(var(--foreground));
    background-color: hsla(var(--foreground), 0.03);
  }

  .sub-nav-btn.active {
    background-color: hsla(var(--primary), 0.1);
    border-color: hsla(var(--primary), 0.3);
    color: hsl(var(--primary));
    font-weight: 700;
  }

  .main-content {
    flex-grow: 1;
    width: 100%;
  }
  ```

- [ ] **Passo 3: Verificar que o projeto React compila totalmente e sem erros de tipos**
  
  Execute sob a pasta `/frontend`:
  ```bash
  npm run build
  ```
  Esperado: Compilação concluída sem nenhum erro no compilador de TypeScript ou do empacotador Vite.

- [ ] **Passo 4: Commit final da integração**
  
  Execute:
  ```bash
  git add frontend/src/views/auth/ frontend/src/App.tsx frontend/src/App.css
  git commit -m "feat: integrar fluxos de login, cadastro, navegacao de abas e rotas"
  ```
