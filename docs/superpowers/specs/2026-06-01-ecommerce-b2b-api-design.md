# Especificação: API de E-commerce B2B

## Visão Geral

API REST multi-tenant para e-commerce B2B, servindo como backend para múltiplos sistemas web. Gerencia transações de compra/venda e anúncio de produtos entre empresas.

## Stack Tecnológica

- **Runtime:** .NET 9
- **Linguagem:** C#
- **Banco de dados:** PostgreSQL
- **ORM:** Entity Framework Core
- **Autenticação:** ASP.NET Core Identity + JWT (com roles, claims, refresh token, lockout, e-mail de confirmação)
- **Arquitetura:** Clean Architecture
- **Estilo API:** REST com Controllers

## Arquitetura

Clean Architecture com 4 projetos + testes:

```
EcommerceB2B.sln
└── src/
    ├── EcommerceB2B.Domain/        # Entidades, enums, interfaces, regras de negócio
    ├── EcommerceB2B.Application/   # Casos de uso, DTOs, interfaces de serviço
    ├── EcommerceB2B.Infrastructure/ # EF Core, repositórios, JWT, migrations, middleware
    └── EcommerceB2B.Api/           # Controllers, DI, pipeline de middleware
└── tests/
    └── EcommerceB2B.Domain.Tests/  # Testes unitários das regras de domínio
```

- **Domain** não depende de nada externo.
- **Application** depende apenas de Domain.
- **Infrastructure** depende de Domain e Application.
- **Api** depende de todos (entry point, compõe a aplicação via DI).

## Modelo Multi-Tenant

Cada requisição autenticada carrega o `CompanyId` no token JWT. Middleware de tenant extrai e expõe o ID para as camadas inferiores. Todas as queries são automaticamente filtradas por tenant — um usuário nunca vê dados de outra empresa.

## Entidades

### Identidade e Acesso

Usa ASP.NET Core Identity como base, que fornece:

| Recurso Identity | Descrição |
|---|---|
| **IdentityUser\<Guid\>** | Tabela de usuários (Id, UserName, Email, PasswordHash, PhoneNumber, etc.) gerenciada pelo Identity. |
| **IdentityRole\<Guid\>** | Tabela de roles (Admin, Comprador, Vendedor). |
| **IdentityUserRole** | Associação muitos-para-muitos entre User e Role. |
| **IdentityUserClaim** | Claims customizadas por usuário. |
| **IdentityUserToken** | Tokens para reset de senha, confirmação de e-mail, etc. |
| **IdentityUserLogin** | Login externo (Google, etc.) — configurado mas opcional. |

Entidades customizadas da aplicação que estendem o Identity:

| Entidade | Descrição |
|---|---|
| **Company** | Empresa no sistema. `Id`, `Name`, `Document` (CNPJ), `Type` (Comprador \| Vendedor \| Ambos), `IsActive`, `CreatedAt`. |
| **CompanyUser** | Tabela de junção vinculando `IdentityUser` a `Company`. `UserId` (FK → IdentityUser), `CompanyId` (FK → Company), `IsActive`. Um usuário pertence a exatamente uma empresa. |

Regras:
- Todo usuário (IdentityUser) pertence obrigatoriamente a uma empresa via CompanyUser.
- Senhas, hash, lockout, confirmação de e-mail, recuperação de senha: tudo gerenciado pelo Identity (não implementamos manualmente).
- JWT inclui claims: `sub` (UserId), `company_id` (CompanyId), `role` (Role).
- Refresh token armazenado no servidor para renovar JWT sem re-login.
- Admin pode gerenciar usuários da própria empresa.
- Registro exige confirmação de e-mail (envio de e-mail simulado/fake em desenvolvimento).

### Catálogo de Produtos

| Entidade | Descrição |
|---|---|
| **Category** | `Id`, `Name`, `Description`, `IsActive` |
| **Product** | `Id`, `CompanyId` (vendedor), `CategoryId`, `Name`, `Description`, `Sku`, `BasePrice`, `StockQuantity`, `IsActive`, `CreatedAt` |
| **ProductPrice** | `Id`, `ProductId`, `CompanyId` (comprador), `CustomPrice`, `MinQuantity`, `IsActive` |

Regras:
- Apenas vendedores (ou usuários com Role=Vendedor da empresa) podem criar/editar produtos.
- `ProductPrice` permite preços diferenciados por empresa compradora e por quantidade mínima de lote.
- `Sku` deve ser único dentro da empresa vendedora.

### Pedidos

| Entidade | Descrição |
|---|---|
| **Order** | `Id`, `BuyerCompanyId`, `SellerCompanyId`, `Status` (Pendente \| Confirmado \| Cancelado \| Enviado \| Entregue), `TotalAmount`, `CreatedAt`, `UpdatedAt` |
| **OrderItem** | `Id`, `OrderId`, `ProductId`, `Quantity`, `UnitPrice`, `TotalPrice` |

Regras:
- Pedido vincula duas empresas: compradora e vendedora.
- `OrderItem.UnitPrice` é snapshot do preço no momento da compra (histórico imutável).
- Fluxo de status: Pendente → Confirmado → Enviado → Entregue. Cancelado pode vir de Pendente ou Confirmado.
- Apenas o vendedor pode avançar o status.
- Apenas o comprador pode criar o pedido.

## Endpoints da API

### Autenticação
```
POST /api/auth/register           → registra empresa + admin (envia e-mail de confirmação)
POST /api/auth/confirm-email      → confirma e-mail via token
POST /api/auth/login              → retorna JWT + refresh token
POST /api/auth/refresh            → renova JWT usando refresh token
POST /api/auth/forgot-password    → envia token de reset de senha por e-mail
POST /api/auth/reset-password     → redefine senha usando token
POST /api/auth/logout             → invalida refresh token
```

### Empresa
```
GET  /api/companies/{companyId}   → dados da empresa
PUT  /api/companies/{companyId}   → atualizar dados (admin)
POST /api/companies/{id}/users    → convidar usuário (admin)
GET  /api/companies/{id}/users    → listar usuários (admin)
```

### Categorias
```
GET    /api/categories       → listar categorias
POST   /api/categories       → criar categoria (vendedor/admin)
PUT    /api/categories/{id}   → editar categoria
```

### Produtos
```
GET    /api/products            → listar (filtros: categoryId, sellerId, minPrice, maxPrice)
GET    /api/products/{id}       → detalhe do produto
POST   /api/products            → anunciar produto (vendedor)
PUT    /api/products/{id}       → editar produto (vendedor)
DELETE /api/products/{id}       → desativar produto (vendedor)
POST   /api/products/{id}/prices → definir preço customizado (vendedor)
```

### Pedidos
```
GET   /api/orders              → listar pedidos da empresa (query param: role=buyer|seller)
GET   /api/orders/{id}          → detalhe do pedido
POST  /api/orders               → criar pedido (comprador)
PATCH /api/orders/{id}/status   → atualizar status (vendedor)
```

Todas as rotas (exceto auth) exigem JWT. Paginação via `page` e `pageSize` nas listagens.

## Tratamento de Erros

- Respostas padronizadas: `{ "error": "mensagem", "details": [...] }`
- 400: validação/regra de negócio
- 401/403: autenticação/autorização
- 404: recurso não encontrado
- 500: erro interno (logado, mensagem genérica ao cliente)

## Testes

- Projeto `EcommerceB2B.Domain.Tests` com xUnit.
- Foco inicial: regras de domínio (validação de entidades, transições de status do pedido).
- Sem dependências externas (testes de unidade puros).

## Observações

- **Comentários educacionais obrigatórios:** todo código (classes, métodos, propriedades, validações, configurações, middlewares, repositórios, controllers — absolutamente tudo) deve ser comentado linha a linha em português (pt-br). Os comentários devem explicar **o que** a linha faz e **por que** ela é necessária, com tom didático voltado para um desenvolvedor júnior. Nenhuma linha de código pode ficar sem comentário. Exemplo esperado:

```csharp
// O construtor recebe o nome e documento da empresa como parâmetros obrigatórios
// pois toda empresa precisa ter esses dados para ser criada no sistema
public Company(string name, string document)
{
    // Atribui o nome recebido à propriedade Name da entidade
    Name = name;

    // Atribui o documento (CNPJ) recebido à propriedade Document
    Document = document;

    // Toda empresa começa como ativa por padrão ao ser criada
    IsActive = true;
}
```

- Commits e documentação em pt-br.
- O MVP cobre Identidade + Catálogo + Pedidos. Pagamentos, estoque avançado e notificações serão adicionados posteriormente.
