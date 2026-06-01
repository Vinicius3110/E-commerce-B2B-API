# E-commerce B2B API

API de marketplace B2B multi-tenant construída com **.NET 9** e **Clean Architecture**. Permite que múltiplas empresas comprem e vendam produtos na mesma plataforma, com autenticação JWT, catálogo de produtos e máquina de estados de pedidos.

---

## Arquitetura

```
┌─────────────────────────────────────────────┐
│                  Api Layer                  │
│  Controllers · Middleware · Program.cs      │
├─────────────────────────────────────────────┤
│              Application Layer              │
│  Use Cases · DTOs · Interfaces de Serviço   │
├─────────────────────────────────────────────┤
│               Domain Layer                  │
│  Entidades · Enums · Exceções · Interfaces  │
├─────────────────────────────────────────────┤
│            Infrastructure Layer             │
│  EF Core · PostgreSQL · JWT · Identity      │
└─────────────────────────────────────────────┘
```

- **Domain** — coração do sistema. Entidades ricas com regras de negócio, validações e máquina de estados. Zero dependências externas.
- **Application** — orquestra os casos de uso. Recebe DTOs, coordena repositórios e retorna resultados.
- **Infrastructure** — implementa persistência (EF Core + PostgreSQL), autenticação (JWT + Identity) e serviços externos.
- **Api** — expõe os endpoints REST. Autentica, extrai tenant do JWT e delega para os serviços de aplicação.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 9 |
| Linguagem | C# 13 |
| Banco de Dados | PostgreSQL |
| ORM | Entity Framework Core 9 |
| Autenticação | ASP.NET Core Identity + JWT Bearer |
| Refresh Token | In-memory com rotação |
| Documentação | Swagger / OpenAPI |
| Testes | xUnit (56 testes unitários) |

---

## Funcionalidades

### Autenticação e Identidade
- Registro de empresa + usuário administrador em um único fluxo
- Login com JWT (access token 60min + refresh token 7 dias)
- Rotação de refresh token (cada uso invalida o anterior)
- Confirmação de e-mail, recuperação de senha
- Identity com GUIDs, políticas de senha e lockout

### Multi-Tenancy
- Claim `company_id` no JWT identifica a empresa autenticada
- `TenantMiddleware` extrai o company_id por requisição
- Controllers usam `GetCompanyId()` para garantir isolamento entre empresas

### Catálogo de Produtos
- CRUD completo com soft delete (`IsActive`)
- Filtros por categoria, empresa vendedora e faixa de preço
- Paginação genérica (`PaginatedResult<T>`)
- Preços customizados B2B por empresa compradora com quantidade mínima

### Pedidos de Compra
- Máquina de estados: `Pendente → Confirmado → Enviado → Entregue`
- Cancelamento permitido apenas nos estados Pendente ou Confirmado
- Empresa compradora cria o pedido; empresa vendedora gerencia o status
- Validação: não pode comprar de si mesmo, pedido exige ao menos 1 item
- Cálculo automático do valor total

---

## Endpoints da API

### Auth — `api/auth` *(público)*

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/auth/register` | Registra empresa + admin |
| POST | `/api/auth/login` | Login (retorna JWT) |
| POST | `/api/auth/refresh` | Rotaciona refresh token |
| POST | `/api/auth/confirm-email` | Confirma e-mail |
| POST | `/api/auth/forgot-password` | Solicita recuperação |
| POST | `/api/auth/reset-password` | Reseta senha |
| POST | `/api/auth/logout` | Revoga refresh token |

### Companies — `api/companies` *(autenticado)*

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/companies/{id}` | Detalhes da empresa |
| PUT | `/api/companies/{id}` | Atualizar nome/tipo |
| POST | `/api/companies/{id}/users` | Vincular usuário |
| GET | `/api/companies/{id}/users` | Listar usuários |

### Categories — `api/categories` *(leitura pública)*

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/categories` | Listar categorias ativas |
| GET | `/api/categories/{id}` | Detalhes da categoria |
| POST | `/api/categories` | Criar categoria *(auth)* |
| PUT | `/api/categories/{id}` | Atualizar categoria *(auth)* |

### Products — `api/products` *(autenticado)*

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/products` | Listar com filtros e paginação |
| GET | `/api/products/{id}` | Detalhes do produto |
| POST | `/api/products` | Criar produto |
| PUT | `/api/products/{id}` | Atualizar produto |
| DELETE | `/api/products/{id}` | Soft delete |
| POST | `/api/products/{id}/prices` | Preço customizado B2B |

### Orders — `api/orders` *(autenticado)*

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/orders?role=buyer\|seller` | Listar pedidos (paginado) |
| GET | `/api/orders/{id}` | Detalhes do pedido |
| POST | `/api/orders` | Criar pedido |
| PATCH | `/api/orders/{id}/status` | Atualizar status (vendedor) |

---

## Estrutura do Projeto

```
src/
├── EcommerceB2B.Api/            # REST API, Controllers, Middleware
├── EcommerceB2B.Application/    # Casos de uso, DTOs, Interfaces
├── EcommerceB2B.Domain/         # Entidades, Enums, Exceções
├── EcommerceB2B.Infrastructure/ # EF Core, Identity, JWT, Repositories
tests/
└── EcommerceB2B.Domain.Tests/   # 56 testes unitários (xUnit)
```

---

## Executando o Projeto

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (rodando na porta 5432)

### Configuração

1. Clone o repositório:
   ```bash
   git clone https://github.com/Vinicius3110/E-commerce-B2B-API.git
   cd E-commerce-B2B-API
   ```

2. Ajuste a connection string em `src/EcommerceB2B.Api/appsettings.Development.json` se necessário:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=EcommerceB2B_Dev;Username=postgres;Password=sua-senha"
   }
   ```

3. Execute a API:
   ```bash
   dotnet run --project src/EcommerceB2B.Api
   ```

4. Acesse o Swagger: [http://localhost:5012/swagger](http://localhost:5012/swagger)

O banco de dados é criado automaticamente na primeira execução via `EnsureCreated()`.

### Testes

```bash
dotnet test
```

56 testes unitários cobrindo as entidades do domínio: `Company`, `Product`, `Order` e `OrderItem`.

---

## Status do Projeto

| Área | Status |
|------|--------|
| Domain (entidades + regras) | ✅ Completo |
| Infrastructure (EF Core + Identity + JWT) | ✅ Completo |
| Application (serviços + DTOs) | ✅ Completo |
| API (controllers + middleware) | ✅ Completo |
| Testes unitários de domínio | ✅ 56 testes |
| Testes de integração | ⬜ Pendente |
| Testes de aplicação | ⬜ Pendente |
| Migrations (EF Core) | ⬜ Pendente (atual: EnsureCreated) |

---

## Licença

Projeto pessoal para fins de aprendizado.
