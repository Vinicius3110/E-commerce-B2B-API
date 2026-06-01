# Especificação de Design: Frontend E-commerce B2B

Este documento detalha o design técnico, de interface e os fluxos de usuário para a construção do **Frontend do E-commerce B2B**, um projeto Single Page Application (SPA) desacoplado que consumirá a API B2B baseada em .NET 9.

---

## 1. Visão Geral e Objetivos

O objetivo deste projeto é construir uma interface de usuário moderna, robusta e esteticamente premium para permitir a interação de empresas compradoras e vendedoras dentro do ecossistema de marketplace multi-tenant.

### Premissas e Decisões de Arquitetura:
* **Estrutura:** Abordagem "Monorepo" (pasta `frontend/` na raiz do repositório atual), facilitando o desenvolvimento integrado e versionamento em repositório único.
* **Stack Principal:** React + Vite + TypeScript.
* **Estilização:** Vanilla CSS (CSS puro) utilizando CSS Variables (Custom Properties) e o modelo de cores HSL para suportar transição dinâmica de cores e efeitos avançados de layout (como Glassmorphism).
* **Navegação Dinâmica:** A interface se adaptará com base em um alternador deslizante no cabeçalho, permitindo mudar dinamicamente entre o **Modo Comprador** (cor de sotaque Violeta) e o **Modo Vendedor** (cor de sotaque Esmeralda).

---

## 2. Estrutura de Diretórios Proposta

O projeto frontend será estruturado de forma a isolar as responsabilidades e garantir código modular e fácil de testar.

```text
frontend/
├── public/                 # Favicon, fontes locais e imagens estáticas
├── src/
│   ├── assets/             # Imagens e ícones importados nos componentes
│   ├── components/         # Componentes UI organizados por contexto
│   │   ├── common/         # Componentes reutilizáveis básicos (Button, Input, Badge, Card, Modal)
│   │   ├── layout/         # Componentes estruturais e fixos (Header, Footer)
│   │   └── features/       # Componentes ricos com lógica (ProductCard, OrderTimeline, CartSummary)
│   ├── contexts/           # Provedores de estado global (AuthContext, CartContext)
│   ├── hooks/              # Custom React Hooks (useAuth, useCart, useApi)
│   ├── services/           # Camada de comunicação de rede (API Client e Endpoints)
│   │   ├── api.ts          # Configuração base de requisições com interceptores
│   │   ├── auth.ts         # Serviços para login, registro de empresa + admin e tokens
│   │   ├── products.ts     # CRUD de produtos e preços diferenciados B2B
│   │   └── orders.ts       # Criação de pedidos e atualização da máquina de estados
│   ├── views/              # Telas e contêineres principais
│   │   ├── auth/           # Login, Cadastro, Recuperação de Senha
│   │   ├── buyer/          # Catálogo, Detalhe de Produto, Carrinho, Pedidos do Comprador
│   │   └── seller/         # Lista de Produtos Próprios, Cadastro/Edição, Vendas e Alteração de Status
│   ├── styles/             # Arquivos de estilo
│   │   ├── variables.css   # Variáveis globais de cores, fontes, bordas e efeitos (Design System)
│   │   ├── global.css      # Reset, estilos gerais, classes de utilidade
│   │   └── [component].css # Estilos locais para os componentes
│   ├── App.tsx             # Arquivo raiz com provedores e rotas
│   └── main.tsx            # Ponto de entrada da aplicação
├── tsconfig.json           # Configurações TypeScript
└── vite.config.ts          # Configurações do empacotador Vite
```

---

## 3. Fluxo de Dados e Estado Global

### A. Autenticação e Multi-Tenancy (`AuthContext`)
* **Estado:** Mantém o usuário atual logado, o JWT token (`access_token`) ativo e dados da empresa.
* **JWT Interceptor:** O cliente `services/api.ts` interceptará todas as requisições para injetar o cabeçalho `Authorization: Bearer <JWT>`. A API backend lê o `company_id` (Tenant) diretamente das Claims do JWT para isolamento das operações de dados.
* **Sessão:** Rotação de `refresh token` nativa será implementada. Caso a API retorne `401 Unauthorized` devido à expiração do access token (durabilidade de 60min), o cliente tentará de forma transparente renovar a sessão utilizando o refresh token persistido e re-executará a requisição original.

### B. Carrinho de Compras B2B (`CartContext`)
* **Estado:** Mantém uma lista em memória dos itens adicionados (`productId`, `name`, `sellerCompanyName`, `price`, `b2bMinQty`, `b2bPrice`, `qty`).
* **Validações Locais (Regras de Negócio B2B):**
  1. **Auto-Compra Bloqueada:** Compradores não podem adicionar ao carrinho produtos que pertençam à sua própria empresa (`sellerCompanyId !== loggedInCompanyId`).
  2. **Quantidade Mínima B2B:** O carrinho calculará dinamicamente a aplicação do preço diferenciado de atacado baseado na quantidade inserida comparada à quantidade mínima configurada no produto.
  3. **Visualizador de Economia:** Exibirá em tempo real quanto a empresa está economizando com o lote em atacado para impulsionar a conversão.

---

## 4. Design System & Identidade Visual (Vanilla CSS)

O design usará variáveis CSS declaradas no escopo `:root` para centralizar a paleta de cores baseada em HSL e permitir a mudança dinâmica de "sotaque" entre comprador e vendedor.

### Ficha de Cores & Tokens (CSS Variables)

```css
:root {
  /* Fundo e Superfícies (Escuro Premium Slate/Zinc) */
  --background: 240 10% 3.9%;       /* #09090b (Preto profundo) */
  --foreground: 0 0% 98%;           /* #fafafa (Off-white de leitura suave) */
  --card: 240 10% 5.9%;             /* #0e0e11 (Cinza grafite escuro para cards) */
  --card-foreground: 0 0% 98%;
  --border: 240 5.9% 15%;           /* Bordas elegantes e discretas */
  --input: 240 5.9% 10%;            /* Inputs de formulário */

  /* Cores de Ação Padrão (Tema Comprador / Violeta) */
  --accent-buyer: 262 83% 58%;      /* Violeta Vibrante */
  --accent-buyer-hover: 262 83% 52%;

  /* Cores de Ação (Tema Vendedor / Esmeralda) */
  --accent-seller: 142 71% 45%;     /* Verde Dinâmico */
  --accent-seller-hover: 142 71% 39%;

  /* Variável Dinâmica controlada via classe no body (.theme-buyer / .theme-seller) */
  --primary: var(--accent-buyer);
  --primary-hover: var(--accent-buyer-hover);

  /* Status da Máquina de Estados (Cores Semânticas) */
  --status-pending: 38 92% 50%;      /* Amarelo Âmbar (Pendente) */
  --status-confirmed: 217 91% 60%;   /* Azul (Confirmado) */
  --status-shipped: 262 83% 58%;     /* Violeta (Enviado) */
  --status-delivered: 142 71% 45%;   /* Verde Esmeralda (Entregue) */
  --status-cancelled: 0 84% 60%;     /* Vermelho (Cancelado) */

  /* Tipografia */
  --font-family: 'Outfit', 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  
  /* Efeitos */
  --radius: 12px;
  --transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  --glass-bg: rgba(14, 14, 17, 0.7);
  --glass-border: rgba(255, 255, 255, 0.08);
  --shadow: 0 4px 30px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.05);
}
```

### Efeitos de Interface Premium
* **Glassmorphism:** Cabeçalho, sidebar e janelas modais utilizarão o efeito de vidro transparente para maior sofisticação de camadas:
  ```css
  background: var(--glass-bg);
  backdrop-filter: blur(12px);
  border: 1px solid var(--glass-border);
  ```
* **Tipografia Elegante:** Títulos usarão a fonte **Outfit** (letras modernas, geométricas e corporativas) e textos contínuos usarão **Inter** (alta legibilidade em telas pequenas).
* **Feedback de Interação (Micro-Animações):**
  * Botões e abas terão efeito `scale(0.98)` ao serem pressionados.
  * Cards de produtos expandirão uma borda levemente brilhante no tom `--primary` e subirão suavemente `4px` sob hover.

---

## 5. Fluxo de Telas e Navegação

### A. Fluxo Autenticação (`/auth`)
1. **Página de Login (`/auth/login`):** Tela minimalista com fundo escuro e degradê sutil do roxo ao verde. Formulário com e-mail e senha. Link para "Registrar Nova Empresa".
2. **Registro de Empresa + Admin (`/auth/register`):** Fluxo unificado contendo dados da empresa (Razão Social, CNPJ/Identificador, e-mail corporativo) e dados do usuário Administrador (Nome, senha, e-mail). Ao registrar com sucesso na API, realiza o login imediato.

### B. O Cabeçalho (Header Dinâmico)
Elemento unificado fixo com efeito blur (vidro). Contém:
1. **Nome da Empresa:** *"Tenant: [Nome da Empresa]"*.
2. **Alternador Deslizante:** Botão de alternância com efeito de deslizamento que altera o tema global:
   * **Se Modo Compras:** Injeta `.theme-buyer` no contêiner principal, alterando as variáveis de cor para **Violeta**.
   * **Se Modo Vendas:** Injeta `.theme-seller` no contêiner principal, alterando as variáveis de cor para **Esmeralda**.
3. **Botão de Logout** e resumo de perfil.

### C. Portal do Comprador (Modo Compras)
1. **Catálogo de Produtos (`/buyer/products`):**
   * Grade de produtos ativos com cards interativos.
   * Filtros laterais elegantes (busca, categoria e valor máximo).
   * **Preços B2B em destaque:** Identificadores visuais mostram claramente o benefício da compra em lotes.
   * Detalhes do produto exibidos em um modal deslizante moderno (Gaveta/Drawer lateral) contendo a descrição completa, termos de entrega do vendedor e o configurador de quantidade com o botão de compra.
2. **Carrinho de Compras (`/buyer/cart`):**
   * Detalhamento dos produtos selecionados.
   * Alertas automáticos sobre quantidade mínima B2B e o total acumulado calculado.
   * Botão finalizador que cria o pedido na API B2B.
3. **Meus Pedidos (`/buyer/orders`):**
   * Histórico de pedidos realizados.
   * Ao clicar em um pedido, abre o detalhamento contendo o **Horizontal Stepper (Timeline de Status)** dinâmico:
     * `[Pendente]` (Cinza/Amarelo) -> `[Confirmado]` (Azul) -> `[Enviado]` (Violeta) -> `[Entregue]` (Verde).
   * Botão de "Cancelar Pedido" ativado apenas se o status do pedido for `Pendente` ou `Confirmado`.

### D. Portal do Vendedor (Modo Vendas)
1. **Gerenciamento do Catálogo Próprio (`/seller/products`):**
   * Tabela visual premium mostrando os produtos criados pela empresa logada.
   * Interruptor (`Toggle Switch`) de ativação/desativação rápida (atualizando o status `IsActive` via API - soft delete).
   * Botão de cadastrar produto que abre um formulário detalhado, incluindo a seção de definição de Regras de Preço B2B (Quantidade Mínima e Preço com Desconto).
2. **Pedidos de Venda Recebidos (`/seller/orders`):**
   * Central de monitoramento contendo as requisições de compras de outras empresas parceiras.
   * **Botões de Transição de Status:** Conforme o pedido progride, o vendedor tem um botão de ação com transições de cor baseadas no estado da máquina de negócios:
     * Se estado for *Pendente* → Exibe botão **"Confirmar Recebimento"** (transiciona status do pedido para `Confirmado`).
     * Se estado for *Confirmado* → Exibe botão **"Despachar Carga / Enviar"** (transiciona status do pedido para `Enviado`).
     * Se estado for *Enviado* → Exibe botão **"Confirmar Entrega"** (transiciona status do pedido para `Entregue`).

---

## 6. Plano de Validação e Verificação

Para garantir a qualidade, o frontend passará pelos seguintes testes de integridade:

### Testes Manuais de Integração:
1. **Fluxo de Tenant Isolado:** Garantir que um usuário de uma empresa A logado no sistema comprador visualize apenas produtos de terceiros e, ao alternar para vendas, visualize apenas os seus próprios produtos.
2. **Validação de Preço de Lote:** Testar no carrinho a transição de preços: por exemplo, adicionar 9 unidades de um produto e ver o preço unitário normal; adicionar a 10ª unidade (limite mínimo) e observar em tempo real a mudança visual e o recálculo do valor unitário e total para o valor com desconto B2B.
3. **Progressão de Pedido (Frente a Frente):** Abrir duas janelas do navegador (uma com Empresa Compradora e outra com Empresa Vendedora):
   * Compradora cria o pedido.
   * Vendedora vê o pedido pendente em tempo real, aceita o pedido (muda para Confirmado).
   * Compradora verifica na sua tela a timeline do pedido mudar de Pendente para Confirmado.
   * Vendedora despacha e depois entrega. O comprador acompanha a mudança de cores na timeline em tempo real.
4. **Soft Delete Act:** Desativar um produto no painel de vendas e garantir que ele desapareça imediatamente no catálogo de compras de outras empresas.

### Verificação de Performance e Acessibilidade:
* Executar testes de auditoria Lighthouse no navegador Chrome, objetivando score > 90 para performance, acessibilidade e SEO.
* Garantir que todas as transições e animações rodem a 60 FPS estáveis nos navegadores desktop e mobile (responsividade completa).
