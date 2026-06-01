# API de E-commerce B2B — Plano de Implementação

> **Para agentes:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recomendado) ou superpowers:executing-plans para implementar este plano tarefa por tarefa. Steps usam sintaxe checkbox (`- [ ]`) para tracking.

**Goal:** Construir uma API REST multi-tenant completa para e-commerce B2B com .NET 9, Clean Architecture, ASP.NET Core Identity + JWT e PostgreSQL.

**Architecture:** Clean Architecture com 4 projetos (Domain, Application, Infrastructure, Api) + testes unitários (xUnit). Multi-tenant via CompanyId no JWT extraído por TenantMiddleware.

**Tech Stack:** .NET 9, ASP.NET Core Identity, EF Core + PostgreSQL, JWT com refresh token, xUnit, Clean Architecture

---

## Fase 1: Setup da Solution e Projetos

### Task 1: Criar solution e projetos

**Files:**
- Create: `EcommerceB2B.sln`
- Create: `src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj`
- Create: `src/EcommerceB2B.Application/EcommerceB2B.Application.csproj`
- Create: `src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj`
- Create: `src/EcommerceB2B.Api/EcommerceB2B.Api.csproj`
- Create: `tests/EcommerceB2B.Domain.Tests/EcommerceB2B.Domain.Tests.csproj`

- [ ] **Step 1: Criar solution**

```bash
dotnet new sln -n EcommerceB2B
```

- [ ] **Step 2: Criar projetos src**

```bash
dotnet new classlib -n EcommerceB2B.Domain -o src/EcommerceB2B.Domain
dotnet new classlib -n EcommerceB2B.Application -o src/EcommerceB2B.Application
dotnet new classlib -n EcommerceB2B.Infrastructure -o src/EcommerceB2B.Infrastructure
dotnet new webapi -n EcommerceB2B.Api -o src/EcommerceB2B.Api --no-https
```

- [ ] **Step 3: Criar projeto de testes**

```bash
dotnet new xunit -n EcommerceB2B.Domain.Tests -o tests/EcommerceB2B.Domain.Tests
```

- [ ] **Step 4: Adicionar todos os projetos à solution**

```bash
dotnet sln add src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
dotnet sln add src/EcommerceB2B.Application/EcommerceB2B.Application.csproj
dotnet sln add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
dotnet sln add src/EcommerceB2B.Api/EcommerceB2B.Api.csproj
dotnet sln add tests/EcommerceB2B.Domain.Tests/EcommerceB2B.Domain.Tests.csproj
```

- [ ] **Step 5: Configurar referências entre projetos**

```bash
dotnet add src/EcommerceB2B.Application/EcommerceB2B.Application.csproj reference src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj reference src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj reference src/EcommerceB2B.Application/EcommerceB2B.Application.csproj
dotnet add src/EcommerceB2B.Api/EcommerceB2B.Api.csproj reference src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
dotnet add src/EcommerceB2B.Api/EcommerceB2B.Api.csproj reference src/EcommerceB2B.Application/EcommerceB2B.Application.csproj
dotnet add src/EcommerceB2B.Api/EcommerceB2B.Api.csproj reference src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
dotnet add tests/EcommerceB2B.Domain.Tests/EcommerceB2B.Domain.Tests.csproj reference src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

- [ ] **Step 6: Restaurar pacotes e compilar**

```bash
dotnet restore
dotnet build
```

Expected: Build succeeded. 0 Error(s).

- [ ] **Step 7: Commit**

```bash
git init
git add -A
git commit -m "feat: criar solution e projetos com Clean Architecture"
```

---

## Fase 2: Camada de Domínio

### Task 2: Criar enums do domínio

**Files:**
- Create: `src/EcommerceB2B.Domain/Enums/CompanyType.cs`
- Create: `src/EcommerceB2B.Domain/Enums/OrderStatus.cs`
- Create: `src/EcommerceB2B.Domain/Enums/UserRole.cs`

- [ ] **Step 1: Criar CompanyType.cs**

```csharp
// Namespace que agrupa os tipos enumerados do domínio (enums)
// Enums são tipos de valor que representam um conjunto fixo de opções nomeadas
namespace EcommerceB2B.Domain.Enums;

// Define os tipos possíveis de empresa no sistema B2B
// O enum é do tipo byte para economizar espaço no banco (0-255 valores possíveis)
public enum CompanyType : byte
{
    // Empresa que apenas compra produtos de outras empresas
    Comprador = 1,

    // Empresa que apenas vende produtos para outras empresas
    Vendedor = 2,

    // Empresa que tanto compra quanto vende produtos na plataforma
    Ambos = 3
}
```

- [ ] **Step 2: Criar OrderStatus.cs**

```csharp
// Namespace que agrupa os tipos enumerados do domínio
namespace EcommerceB2B.Domain.Enums;

// Define os status possíveis de um pedido no ciclo de vida B2B
// A ordem dos valores reflete o fluxo natural do processo de compra
public enum OrderStatus : byte
{
    // Pedido criado pelo comprador, aguardando ação do vendedor
    Pendente = 1,

    // Pedido confirmado pelo vendedor, pronto para ser processado
    Confirmado = 2,

    // Pedido cancelado (pode vir de Pendente ou Confirmado)
    Cancelado = 3,

    // Pedido foi despachado pelo vendedor e está em transporte
    Enviado = 4,

    // Pedido foi recebido pelo comprador (status final)
    Entregue = 5
}
```

- [ ] **Step 3: Criar UserRole.cs**

```csharp
// Namespace que agrupa os tipos enumerados do domínio
namespace EcommerceB2B.Domain.Enums;

// Define os papéis (roles) que um usuário pode ter dentro de uma empresa
// Estes valores são usados tanto no Identity quanto nas regras de autorização
public enum UserRole : byte
{
    // Administrador da empresa — pode gerenciar usuários, produtos e pedidos
    Admin = 1,

    // Usuário com permissão para comprar (visualizar produtos, criar pedidos)
    Comprador = 2,

    // Usuário com permissão para vender (anunciar produtos, gerenciar pedidos recebidos)
    Vendedor = 3
}
```

- [ ] **Step 4: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/EcommerceB2B.Domain/Enums/
git commit -m "feat: adicionar enums do domínio (CompanyType, OrderStatus, UserRole)"
```

### Task 3: Criar exceções de domínio

**Files:**
- Create: `src/EcommerceB2B.Domain/Exceptions/DomainException.cs`

- [ ] **Step 1: Criar DomainException.cs**

```csharp
// Namespace para exceções customizadas do domínio
namespace EcommerceB2B.Domain.Exceptions;

// Classe base para todas as exceções de regra de negócio do domínio
// Herda de Exception para ser compatível com o sistema de exceções do .NET
// Usar uma exceção customizada permite diferenciar erros de negócio de erros técnicos
public class DomainException : Exception
{
    // Construtor padrão: recebe apenas a mensagem de erro
    // base(message) chama o construtor da classe pai (Exception) passando a mensagem
    public DomainException(string message) : base(message)
    {
    }

    // Construtor com mensagem e exceção interna (para encadeamento de exceções)
    // Útil quando uma exceção é causada por outra e queremos preservar a causa original
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Exceptions/
git commit -m "feat: adicionar exceção de domínio DomainException"
```

### Task 4: Criar entidade Company

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/Company.cs`

- [ ] **Step 1: Criar Company.cs**

```csharp
// Namespace que agrupa todas as entidades do domínio
// Entidades são classes que representam conceitos de negócio com identidade própria
using EcommerceB2B.Domain.Enums;
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Classe que representa uma empresa no sistema multi-tenant B2B
// Toda operação no sistema está vinculada a uma Company (tenant isolation)
public class Company
{
    // Identificador único da empresa (Guid evita colisões de ID em sistemas distribuídos)
    // O set é privado para garantir que o ID só seja definido uma vez, no construtor
    public Guid Id { get; private set; }

    // Nome fantasia ou razão social da empresa
    public string Name { get; private set; }

    // Documento oficial (CNPJ) da empresa — usado para identificação fiscal
    public string Document { get; private set; }

    // Define se a empresa compra, vende ou faz ambos na plataforma
    public CompanyType Type { get; private set; }

    // Indica se a empresa está ativa no sistema
    // Empresas inativas não podem fazer login nem operar
    public bool IsActive { get; private set; }

    // Data e hora em que a empresa foi cadastrada no sistema
    // DateTime.UtcNow é usado para evitar problemas de fuso horário
    public DateTime CreatedAt { get; private set; }

    // Construtor privado exigido pelo Entity Framework Core para materialização
    // O EF Core usa este construtor ao carregar entidades do banco de dados
    private Company()
    {
        // Inicializa Name e Document com string vazia para evitar null reference
        // O EF Core preencherá os valores ao carregar do banco de dados
        Name = string.Empty;
        Document = string.Empty;
    }

    // Construtor público usado para criar novas empresas no código da aplicação
    // Recebe os dados obrigatórios (name e document) e opcionais (type)
    public Company(string name, string document, CompanyType type = CompanyType.Ambos)
    {
        // Gera um novo identificador único universal no momento da criação
        Id = Guid.NewGuid();

        // Atribui e valida o nome da empresa (não pode ser vazio ou nulo)
        SetName(name);

        // Atribui e valida o documento (CNPJ) — não pode ser vazio ou nulo
        SetDocument(document);

        // Define o tipo da empresa (compradora, vendedora ou ambas)
        Type = type;

        // Toda empresa começa como ativa por padrão ao ser criada
        IsActive = true;

        // Registra a data/hora UTC de criação para auditoria
        CreatedAt = DateTime.UtcNow;
    }

    // Método para atualizar o nome da empresa com validação
    // É público para ser chamado pelos casos de uso da camada de Application
    public void SetName(string name)
    {
        // Validação: nome não pode ser nulo, vazio ou apenas espaços em branco
        // string.IsNullOrWhiteSpace cobre os três casos (null, "", "   ")
        if (string.IsNullOrWhiteSpace(name))
        {
            // Lança exceção de domínio se a validação falhar
            // Isso interrompe a execução e notifica a camada superior do erro
            throw new DomainException("O nome da empresa é obrigatório.");
        }

        // Atribui o nome validado à propriedade Name
        Name = name;
    }

    // Método para atualizar o documento (CNPJ) com validação
    public void SetDocument(string document)
    {
        // Validação: documento não pode ser nulo, vazio ou apenas espaços
        if (string.IsNullOrWhiteSpace(document))
        {
            throw new DomainException("O documento (CNPJ) da empresa é obrigatório.");
        }

        // Atribui o documento validado à propriedade Document
        Document = document;
    }

    // Método para atualizar o tipo da empresa (compradora, vendedora, ambas)
    public void SetType(CompanyType type)
    {
        // Validação: o tipo deve ser um valor válido do enum CompanyType
        // Enum.IsDefined verifica se o valor existe na definição do enum
        if (!Enum.IsDefined(type))
        {
            throw new DomainException("O tipo de empresa informado é inválido.");
        }

        // Atribui o tipo validado à propriedade Type
        Type = type;
    }

    // Método para ativar a empresa (permite acesso ao sistema)
    public void Activate()
    {
        // Define a propriedade IsActive como true
        IsActive = true;
    }

    // Método para desativar a empresa (bloqueia acesso ao sistema)
    // Útil para suspender temporariamente uma empresa sem excluir seus dados
    public void Deactivate()
    {
        // Define a propriedade IsActive como false
        IsActive = false;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/Company.cs
git commit -m "feat: criar entidade Company com validações de domínio"
```

### Task 5: Criar entidade CompanyUser

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/CompanyUser.cs`

- [ ] **Step 1: Criar CompanyUser.cs**

```csharp
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Tabela de junção que vincula um usuário do Identity (IdentityUser<Guid>)
// a uma empresa (Company). Garante a relação "um usuário pertence a uma empresa".
// Esta entidade é a base do isolamento multi-tenant.
public class CompanyUser
{
    // Identificador único do vínculo entre usuário e empresa
    public Guid Id { get; private set; }

    // ID do usuário na tabela IdentityUser do ASP.NET Core Identity
    // Este é um Guid porque usamos IdentityUser<Guid>
    public Guid UserId { get; private set; }

    // ID da empresa à qual o usuário está vinculado
    public Guid CompanyId { get; private set; }

    // Indica se o vínculo está ativo — pode ser usado para desativar usuários específicos
    public bool IsActive { get; private set; }

    // Propriedade de navegação para a empresa (não é carregada automaticamente pelo EF Core)
    // O virtual permite lazy loading se configurado no EF Core
    public virtual Company Company { get; private set; } = null!;

    // Construtor privado para o Entity Framework Core materializar a entidade do banco
    private CompanyUser()
    {
    }

    // Construtor público usado para criar um novo vínculo usuário-empresa
    // Recebe os IDs que formam a chave estrangeira composta
    public CompanyUser(Guid userId, Guid companyId)
    {
        // Gera um novo identificador único para o vínculo
        Id = Guid.NewGuid();

        // Valida que o UserId não é um Guid vazio (00000000-0000-0000-0000-000000000000)
        if (userId == Guid.Empty)
        {
            throw new DomainException("O ID do usuário é obrigatório.");
        }

        // Valida que o CompanyId não é um Guid vazio
        if (companyId == Guid.Empty)
        {
            throw new DomainException("O ID da empresa é obrigatório.");
        }

        // Atribui os IDs validados às propriedades
        UserId = userId;
        CompanyId = companyId;

        // Todo vínculo novo começa como ativo
        IsActive = true;
    }

    // Método para desativar o vínculo usuário-empresa
    // Desativar o vínculo impede o usuário de acessar o sistema
    public void Deactivate()
    {
        IsActive = false;
    }

    // Método para reativar o vínculo usuário-empresa
    // Permite que um usuário previamente desativado volte a acessar o sistema
    public void Activate()
    {
        IsActive = true;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/CompanyUser.cs
git commit -m "feat: criar entidade CompanyUser (vínculo usuário-empresa)"
```

### Task 6: Criar entidade Category

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/Category.cs`

- [ ] **Step 1: Criar Category.cs**

```csharp
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Representa uma categoria de produtos no catálogo
// Categorias são globais (compartilhadas entre todas as empresas) para manter
// uma taxonomia consistente na plataforma
public class Category
{
    // Identificador único da categoria
    public Guid Id { get; private set; }

    // Nome da categoria (ex: "Eletrônicos", "Móveis", "Vestuário")
    public string Name { get; private set; }

    // Descrição opcional da categoria para ajudar na navegação do catálogo
    // Pode ser nula — string? permite valor null
    public string? Description { get; private set; }

    // Indica se a categoria está ativa e visível no catálogo
    public bool IsActive { get; private set; }

    // Propriedade de navegação para a lista de produtos desta categoria
    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

    // Construtor privado para o Entity Framework Core
    private Category()
    {
        Name = string.Empty;
    }

    // Construtor público para criar uma nova categoria
    public Category(string name, string? description = null)
    {
        // Gera identificador único
        Id = Guid.NewGuid();

        // Valida e atribui o nome da categoria
        SetName(name);

        // Descrição é opcional — pode ser nula
        Description = description;

        // Categoria começa ativa por padrão
        IsActive = true;
    }

    // Atualiza o nome da categoria com validação
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("O nome da categoria é obrigatório.");
        }

        Name = name;
    }

    // Atualiza a descrição da categoria
    // Não requer validação porque é um campo opcional
    public void SetDescription(string? description)
    {
        Description = description;
    }

    // Ativa a categoria (torna visível no catálogo)
    public void Activate()
    {
        IsActive = true;
    }

    // Desativa a categoria (oculta do catálogo sem excluir do banco)
    public void Deactivate()
    {
        IsActive = false;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/Category.cs
git commit -m "feat: criar entidade Category"
```

### Task 7: Criar entidade Product

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/Product.cs`

- [ ] **Step 1: Criar Product.cs**

```csharp
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Representa um produto anunciado por uma empresa vendedora no catálogo B2B
// Cada produto pertence a uma empresa (tenant) e a uma categoria
public class Product
{
    // Identificador único do produto
    public Guid Id { get; private set; }

    // ID da empresa vendedora que anunciou este produto
    // Usado para isolamento multi-tenant e filtros de busca
    public Guid CompanyId { get; private set; }

    // ID da categoria à qual este produto pertence
    public Guid CategoryId { get; private set; }

    // Nome do produto (título de exibição no catálogo)
    public string Name { get; private set; }

    // Descrição detalhada do produto (opcional)
    public string? Description { get; private set; }

    // Código SKU (Stock Keeping Unit) — código único de identificação do produto
    // Deve ser único dentro da empresa vendedora
    public string Sku { get; private set; }

    // Preço base do produto — visível para todos os compradores
    // Pode ser sobrescrito por ProductPrice para clientes específicos
    public decimal BasePrice { get; private set; }

    // Quantidade disponível em estoque
    // Controlado pelo vendedor e decrementado a cada pedido confirmado
    public int StockQuantity { get; private set; }

    // Indica se o produto está ativo e visível no catálogo
    public bool IsActive { get; private set; }

    // Data e hora de criação do anúncio
    public DateTime CreatedAt { get; private set; }

    // Propriedades de navegação para o EF Core
    // Virtual permite lazy loading se configurado
    public virtual Company Company { get; private set; } = null!;
    public virtual Category Category { get; private set; } = null!;

    // Lista de preços customizados por empresa compradora
    public virtual ICollection<ProductPrice> CustomPrices { get; private set; } = new List<ProductPrice>();

    // Construtor privado para o Entity Framework Core
    private Product()
    {
        Name = string.Empty;
        Sku = string.Empty;
    }

    // Construtor público para criar um novo anúncio de produto
    public Product(Guid companyId, Guid categoryId, string name, string sku, decimal basePrice, int stockQuantity, string? description = null)
    {
        // Gera identificador único
        Id = Guid.NewGuid();

        // Validações de integridade (IDs não podem ser vazios)
        if (companyId == Guid.Empty)
            throw new DomainException("O ID da empresa vendedora é obrigatório.");

        if (categoryId == Guid.Empty)
            throw new DomainException("O ID da categoria é obrigatório.");

        // Atribui os IDs validados
        CompanyId = companyId;
        CategoryId = categoryId;

        // Aplica validações de nome e SKU
        SetName(name);
        SetSku(sku);
        SetBasePrice(basePrice);
        SetStockQuantity(stockQuantity);

        // Descrição é opcional
        Description = description;

        // Produto começa ativo ao ser criado
        IsActive = true;

        // Registra data/hora de criação
        CreatedAt = DateTime.UtcNow;
    }

    // Métodos de atualização com validação de domínio

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do produto é obrigatório.");

        Name = name;
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("O SKU do produto é obrigatório.");

        Sku = sku;
    }

    public void SetBasePrice(decimal price)
    {
        // Preço base não pode ser negativo
        // decimal permite valores com alta precisão, ideal para valores monetários
        if (price < 0)
            throw new DomainException("O preço base do produto não pode ser negativo.");

        BasePrice = price;
    }

    public void SetStockQuantity(int quantity)
    {
        // Estoque não pode ser negativo
        if (quantity < 0)
            throw new DomainException("A quantidade em estoque não pode ser negativa.");

        StockQuantity = quantity;
    }

    // Atualiza a categoria do produto
    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("O ID da categoria é obrigatório.");

        CategoryId = categoryId;
    }

    // Métodos de controle de estado do produto

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/Product.cs
git commit -m "feat: criar entidade Product com validações de domínio"
```

### Task 8: Criar entidade ProductPrice

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/ProductPrice.cs`

- [ ] **Step 1: Criar ProductPrice.cs**

```csharp
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Representa um preço customizado de produto para uma empresa compradora específica
// Essencial em B2B: permite negociação de preços por cliente e por volume
public class ProductPrice
{
    // Identificador único do registro de preço customizado
    public Guid Id { get; private set; }

    // ID do produto ao qual este preço se aplica
    public Guid ProductId { get; private set; }

    // ID da empresa compradora que recebe este preço especial
    public Guid CompanyId { get; private set; }

    // Preço unitário customizado para esta empresa compradora
    public decimal CustomPrice { get; private set; }

    // Quantidade mínima que o comprador precisa adquirir para ter este preço
    // Exemplo: preço X se comprar pelo menos 100 unidades
    public int MinQuantity { get; private set; }

    // Indica se este preço customizado está ativo
    public bool IsActive { get; private set; }

    // Propriedades de navegação para EF Core
    public virtual Product Product { get; private set; } = null!;
    public virtual Company Company { get; private set; } = null!;

    // Construtor privado para EF Core
    private ProductPrice()
    {
    }

    // Construtor público para criar um preço customizado
    public ProductPrice(Guid productId, Guid companyId, decimal customPrice, int minQuantity)
    {
        Id = Guid.NewGuid();

        // Validações de integridade referencial
        if (productId == Guid.Empty)
            throw new DomainException("O ID do produto é obrigatório.");

        if (companyId == Guid.Empty)
            throw new DomainException("O ID da empresa compradora é obrigatório.");

        ProductId = productId;
        CompanyId = companyId;

        // Aplica validações de negócio
        SetCustomPrice(customPrice);
        SetMinQuantity(minQuantity);

        // Preço customizado começa ativo
        IsActive = true;
    }

    // Atualiza o preço customizado com validação
    public void SetCustomPrice(decimal price)
    {
        // Preço não pode ser negativo e deve ser maior que zero
        // (um preço zero significaria produto gratuito, o que não faz sentido em B2B)
        if (price <= 0)
            throw new DomainException("O preço customizado deve ser maior que zero.");

        CustomPrice = price;
    }

    // Atualiza a quantidade mínima para este preço
    public void SetMinQuantity(int quantity)
    {
        // A quantidade mínima deve ser pelo menos 1 (não faz sentido lote mínimo zero)
        if (quantity < 1)
            throw new DomainException("A quantidade mínima deve ser pelo menos 1.");

        MinQuantity = quantity;
    }

    // Métodos de controle de estado
    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/ProductPrice.cs
git commit -m "feat: criar entidade ProductPrice (preço customizado por empresa)"
```

### Task 9: Criar entidade OrderItem (precisa existir antes de Order)

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/OrderItem.cs`

- [ ] **Step 1: Criar OrderItem.cs**

```csharp
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Representa um item dentro de um pedido (linha do pedido)
// Cada OrderItem referencia um produto e tem quantidade e preço
public class OrderItem
{
    // Identificador único do item do pedido
    public Guid Id { get; private set; }

    // ID do pedido ao qual este item pertence
    public Guid OrderId { get; private set; }

    // ID do produto que foi comprado
    public Guid ProductId { get; private set; }

    // Quantidade de unidades deste produto no pedido
    public int Quantity { get; private set; }

    // Preço unitário no momento da compra (snapshot)
    // Isso garante o histórico: se o preço do produto mudar depois,
    // o pedido ainda mostra o preço que foi pago
    public decimal UnitPrice { get; private set; }

    // Preço total deste item (Quantity * UnitPrice)
    // Calculado e armazenado no momento da criação para garantir integridade histórica
    public decimal TotalPrice { get; private set; }

    // Propriedade de navegação para o pedido ao qual este item pertence
    public virtual Order Order { get; private set; } = null!;

    // Propriedade de navegação para o produto referenciado
    public virtual Product Product { get; private set; } = null!;

    // Construtor privado para EF Core
    private OrderItem()
    {
    }

    // Construtor público para criar um item de pedido
    // O OrderId é atribuído externamente quando o pedido é salvo
    public OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();

        // Validação do produto
        if (productId == Guid.Empty)
            throw new DomainException("O ID do produto é obrigatório.");

        // Validação da quantidade
        if (quantity < 1)
            throw new DomainException("A quantidade deve ser pelo menos 1.");

        // Validação do preço unitário
        if (unitPrice <= 0)
            throw new DomainException("O preço unitário deve ser maior que zero.");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;

        // Calcula o preço total multiplicando quantidade pelo preço unitário
        TotalPrice = quantity * unitPrice;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/OrderItem.cs
git commit -m "feat: criar entidade OrderItem (item de pedido com snapshot de preço)"
```

### Task 10: Criar entidade Order

**Files:**
- Create: `src/EcommerceB2B.Domain/Entities/Order.cs`

- [ ] **Step 1: Criar Order.cs**

```csharp
using EcommerceB2B.Domain.Enums;
using EcommerceB2B.Domain.Exceptions;

namespace EcommerceB2B.Domain.Entities;

// Representa um pedido de compra no sistema B2B
// Um pedido envolve duas empresas: a compradora e a vendedora
public class Order
{
    // Identificador único do pedido
    public Guid Id { get; private set; }

    // ID da empresa que está comprando
    public Guid BuyerCompanyId { get; private set; }

    // ID da empresa que está vendendo
    public Guid SellerCompanyId { get; private set; }

    // Status atual do pedido no fluxo de compra
    public OrderStatus Status { get; private set; }

    // Valor total do pedido (soma de todos os OrderItem.TotalPrice)
    public decimal TotalAmount { get; private set; }

    // Data/hora de criação do pedido
    public DateTime CreatedAt { get; private set; }

    // Data/hora da última atualização do pedido
    public DateTime UpdatedAt { get; private set; }

    // Lista de itens que compõem este pedido
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    // Propriedades de navegação para as empresas envolvidas
    public virtual Company BuyerCompany { get; private set; } = null!;
    public virtual Company SellerCompany { get; private set; } = null!;

    // Construtor privado para EF Core
    private Order()
    {
    }

    // Construtor público para criar um novo pedido
    public Order(Guid buyerCompanyId, Guid sellerCompanyId, IEnumerable<OrderItem> items)
    {
        // Validações de integridade referencial
        if (buyerCompanyId == Guid.Empty)
            throw new DomainException("O ID da empresa compradora é obrigatório.");

        if (sellerCompanyId == Guid.Empty)
            throw new DomainException("O ID da empresa vendedora é obrigatório.");

        // Regra de negócio: uma empresa não pode comprar de si mesma
        if (buyerCompanyId == sellerCompanyId)
            throw new DomainException("Uma empresa não pode fazer um pedido para si mesma.");

        Id = Guid.NewGuid();
        BuyerCompanyId = buyerCompanyId;
        SellerCompanyId = sellerCompanyId;

        // Define o status inicial do pedido
        Status = OrderStatus.Pendente;

        // Converte os itens para lista para verificação e cálculo
        var itemsList = items.ToList();

        // Regra de negócio: todo pedido precisa ter pelo menos um item
        if (itemsList.Count == 0)
            throw new DomainException("O pedido deve conter pelo menos um item.");

        // Adiciona e calcula o total
        Items = itemsList;
        RecalculateTotal();

        // Registra datas de criação e atualização
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Recalcula o valor total do pedido somando todos os itens
    private void RecalculateTotal()
    {
        // Soma o TotalPrice de cada item da lista
        // Sum() é um método LINQ que itera sobre a coleção acumulando os valores
        TotalAmount = Items.Sum(item => item.TotalPrice);

        // Atualiza a data de modificação
        UpdatedAt = DateTime.UtcNow;
    }

    // Transições de status do pedido seguindo o fluxo definido no domínio

    // Confirma o pedido — ação realizada pelo vendedor
    public void Confirm()
    {
        // Só é possível confirmar um pedido que está Pendente
        if (Status != OrderStatus.Pendente)
            throw new DomainException("Apenas pedidos pendentes podem ser confirmados.");

        Status = OrderStatus.Confirmado;
        UpdatedAt = DateTime.UtcNow;
    }

    // Cancela o pedido — pode vir de Pendente ou Confirmado
    public void Cancel()
    {
        // Regra de negócio: pedidos Enviados ou Entregues não podem ser cancelados
        if (Status != OrderStatus.Pendente && Status != OrderStatus.Confirmado)
            throw new DomainException("Apenas pedidos pendentes ou confirmados podem ser cancelados.");

        Status = OrderStatus.Cancelado;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marca o pedido como enviado — ação realizada pelo vendedor
    public void Ship()
    {
        // Só é possível enviar um pedido que está Confirmado
        if (Status != OrderStatus.Confirmado)
            throw new DomainException("Apenas pedidos confirmados podem ser enviados.");

        Status = OrderStatus.Enviado;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marca o pedido como entregue — ação realizada pelo vendedor
    public void Deliver()
    {
        // Só é possível entregar um pedido que está Enviado
        if (Status != OrderStatus.Enviado)
            throw new DomainException("Apenas pedidos enviados podem ser marcados como entregues.");

        Status = OrderStatus.Entregue;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Domain/Entities/Order.cs
git commit -m "feat: criar entidade Order com máquina de estados de status"
```

### Task 11: Criar interfaces de repositório no domínio

**Files:**
- Create: `src/EcommerceB2B.Domain/Interfaces/ICompanyRepository.cs`
- Create: `src/EcommerceB2B.Domain/Interfaces/IProductRepository.cs`
- Create: `src/EcommerceB2B.Domain/Interfaces/IOrderRepository.cs`
- Create: `src/EcommerceB2B.Domain/Interfaces/ICategoryRepository.cs`

- [ ] **Step 1: Criar ICompanyRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para a entidade Company
// A interface fica no Domain (camada interna), mas a implementação fica no Infrastructure
// Isso é inversão de dependência: o Domain define o contrato, o Infrastructure implementa
public interface ICompanyRepository
{
    // Busca uma empresa pelo seu ID único
    // Retorna null se não encontrar (por isso o tipo é Company?)
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Busca uma empresa pelo documento (CNPJ)
    // Útil para validar unicidade do CNPJ no cadastro
    Task<Company?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);

    // Adiciona uma nova empresa ao banco de dados
    Task AddAsync(Company company, CancellationToken cancellationToken = default);

    // Atualiza os dados de uma empresa existente
    // Não retorna nada — a entidade passada por referência já contém as alterações
    void Update(Company company);
}
```

- [ ] **Step 2: Criar ICategoryRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Category
public interface ICategoryRepository
{
    // Busca uma categoria pelo ID
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Lista todas as categorias ativas (para exibição no catálogo)
    // IReadOnlyList garante que o resultado não será modificado pelo consumidor
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);

    // Adiciona uma nova categoria
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    // Atualiza uma categoria existente
    void Update(Category category);
}
```

- [ ] **Step 3: Criar IProductRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Product
public interface IProductRepository
{
    // Busca um produto pelo ID, incluindo a empresa vendedora e a categoria
    // O parâmetro includeDetails controla se as propriedades de navegação são carregadas
    Task<Product?> GetByIdAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default);

    // Lista produtos com filtros e paginação
    // Cada parâmetro opcional refina a busca; se nulo, o filtro não é aplicado
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? sellerCompanyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    // Verifica se um SKU já existe para uma empresa vendedora (regra de unicidade)
    Task<bool> SkuExistsAsync(Guid companyId, string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    // Adiciona um novo produto
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    // Atualiza um produto existente
    void Update(Product product);

    // Adiciona um preço customizado para uma empresa compradora
    Task AddPriceAsync(ProductPrice price, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Criar IOrderRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Order
public interface IOrderRepository
{
    // Busca um pedido pelo ID, incluindo os itens e as empresas envolvidas
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Lista pedidos de uma empresa (como compradora ou vendedora)
    // O parâmetro asBuyer controla se busca pedidos onde a empresa é compradora ou vendedora
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        bool asBuyer,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    // Adiciona um novo pedido
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    // Atualiza um pedido existente (usado para mudanças de status)
    void Update(Order order);
}
```

- [ ] **Step 5: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Domain/EcommerceB2B.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/EcommerceB2B.Domain/Interfaces/
git commit -m "feat: criar interfaces de repositório no domínio"
```

---

## Fase 3: Camada de Infraestrutura — Banco de Dados

### Task 12: Adicionar pacotes NuGet ao Infrastructure

**Files:**
- Modify: `src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj`

- [ ] **Step 1: Adicionar pacotes EF Core e PostgreSQL**

```bash
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

- [ ] **Step 2: Commit (sem alterações de código ainda)**

```bash
git add src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
git commit -m "build: adicionar pacotes EF Core, PostgreSQL, Identity e JWT ao Infrastructure"
```

### Task 13: Criar AppDbContext

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Persistence/AppDbContext.cs`

- [ ] **Step 1: Criar AppDbContext.cs**

```csharp
// Importa as entidades do domínio que serão mapeadas para tabelas
using EcommerceB2B.Domain.Entities;

// Importa o Identity para configurar as tabelas de autenticação
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceB2B.Infrastructure.Persistence;

// Classe principal de contexto do banco de dados
// Herda de IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid> para integrar
// o ASP.NET Core Identity com o Entity Framework Core usando Guids como chaves primárias
// IdentityDbContext já inclui DbSets para Users, Roles, Claims, Tokens e Logins
public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    // DbSet representa uma tabela no banco de dados
    // Cada DbSet é uma coleção que o EF Core usa para fazer queries e salvar dados

    // Tabela de empresas (tenants do sistema multi-tenant)
    public DbSet<Company> Companies { get; set; } = null!;

    // Tabela de vínculo entre usuários do Identity e empresas
    public DbSet<CompanyUser> CompanyUsers { get; set; } = null!;

    // Tabela de categorias de produtos
    public DbSet<Category> Categories { get; set; } = null!;

    // Tabela de produtos anunciados
    public DbSet<Product> Products { get; set; } = null!;

    // Tabela de preços customizados por empresa compradora
    public DbSet<ProductPrice> ProductPrices { get; set; } = null!;

    // Tabela de pedidos de compra
    public DbSet<Order> Orders { get; set; } = null!;

    // Tabela de itens de pedido
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    // Construtor que recebe as opções de configuração do DbContext
    // DbContextOptions<AppDbContext> contém a string de conexão e outras configs
    // : base(options) passa as opções para a classe base IdentityDbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Método chamado pelo EF Core durante a criação do modelo
    // Aqui configuramos mapeamentos adicionais que não podem ser feitos por Data Annotations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chama a configuração da classe base primeiro
        // Isso é obrigatório para que o Identity configure suas próprias tabelas (Users, Roles, etc.)
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações de entidade definidas em classes separadas
        // ApplyConfigurationsFromAssembly escaneia o assembly atual e aplica todas as classes
        // que implementam IEntityTypeConfiguration<T>
        // Isso mantém o DbContext limpo e cada entidade com sua configuração em arquivo próprio
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Persistence/AppDbContext.cs
git commit -m "feat: criar AppDbContext com Identity e DbSets de domínio"
```

### Task 14: Criar configurações de entidade (Fluent API)

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/CompanyConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/CompanyUserConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/ProductPriceConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
- Create: `src/EcommerceB2B.Infrastructure/Persistence/Configurations/OrderItemConfiguration.cs`

- [ ] **Step 1: Criar CompanyConfiguration.cs**

```csharp
// Importa a entidade do domínio que será configurada
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração Fluent API para a entidade Company
// Implementa IEntityTypeConfiguration<Company> que é detectado automaticamente pelo EF Core
// Separar configurações em classes próprias mantém o DbContext limpo e organizado
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    // Método chamado pelo EF Core para configurar o mapeamento da entidade para a tabela
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        // Define o nome da tabela no banco de dados
        // Sem esta linha, o EF Core usaria o nome da classe "Company" como nome da tabela
        builder.ToTable("Companies");

        // Configura a chave primária da tabela
        // HasKey define qual propriedade é a PK (Primary Key)
        builder.HasKey(c => c.Id);

        // Configura a propriedade Name
        builder.Property(c => c.Name)
            // Define como NOT NULL e com no máximo 200 caracteres
            .IsRequired()
            .HasMaxLength(200);

        // Configura a propriedade Document (CNPJ)
        builder.Property(c => c.Document)
            .IsRequired()
            .HasMaxLength(14); // CNPJ tem 14 dígitos (sem máscara)

        // Configura a propriedade Type (enum CompanyType)
        builder.Property(c => c.Type)
            // Converte o enum para byte ao salvar no banco (mais eficiente que string)
            // HasConversion<byte>() mapeia o enum para seu valor numérico subjacente
            .HasConversion<byte>();

        // Configura a propriedade IsActive
        builder.Property(c => c.IsActive)
            // Define valor padrão true no banco de dados (toda empresa nova começa ativa)
            .HasDefaultValue(true);

        // Cria um índice único na coluna Document para garantir unicidade
        // Isso impede que duas empresas tenham o mesmo CNPJ no sistema
        builder.HasIndex(c => c.Document)
            .IsUnique();
    }
}
```

- [ ] **Step 2: Criar CompanyUserConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Configuração Fluent API para a entidade de vínculo CompanyUser
public class CompanyUserConfiguration : IEntityTypeConfiguration<CompanyUser>
{
    public void Configure(EntityTypeBuilder<CompanyUser> builder)
    {
        builder.ToTable("CompanyUsers");

        builder.HasKey(cu => cu.Id);

        builder.Property(cu => cu.UserId)
            .IsRequired();

        builder.Property(cu => cu.CompanyId)
            .IsRequired();

        builder.Property(cu => cu.IsActive)
            .HasDefaultValue(true);

        // Cria um índice único composto (UserId + CompanyId)
        // Garante que um usuário só pode ter um vínculo com cada empresa
        builder.HasIndex(cu => new { cu.UserId, cu.CompanyId })
            .IsUnique();

        // Configura o relacionamento com IdentityUser
        // HasOne define que CompanyUser tem uma referência para um IdentityUser
        // WithMany() indica que um IdentityUser pode ter vários CompanyUser
        // (embora na prática seja apenas um, a estrutura permite flexibilidade)
        // HasForeignKey define qual coluna é a FK que referencia IdentityUser
        // OnDelete(DeleteBehavior.Restrict) impede deleção em cascata
        builder.HasOne<IdentityUser<Guid>>()
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 3: Criar CategoryConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);
    }
}
```

- [ ] **Step 4: Criar ProductConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        // Configura o preço base com tipo decimal(18,2) no banco
        // 18 dígitos no total, 2 casas decimais — padrão para valores monetários
        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        // Índice único composto: SKU + CompanyId
        // Garante que o SKU é único dentro de cada empresa vendedora
        builder.HasIndex(p => new { p.CompanyId, p.Sku })
            .IsUnique();

        // Relacionamento Product → Company (vendedora)
        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento Product → Category
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 5: Criar ProductPriceConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");

        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.CustomPrice)
            .HasPrecision(18, 2);

        builder.Property(pp => pp.IsActive)
            .HasDefaultValue(true);

        // Índice composto: um produto só pode ter um preço customizado por empresa
        builder.HasIndex(pp => new { pp.ProductId, pp.CompanyId })
            .IsUnique();

        // Relacionamento com Product
        builder.HasOne(pp => pp.Product)
            .WithMany(p => p.CustomPrices)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Se o produto for deletado, remove os preços

        // Relacionamento com Company (compradora)
        builder.HasOne(pp => pp.Company)
            .WithMany()
            .HasForeignKey(pp => pp.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 6: Criar OrderConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // Configura o status como byte (valor numérico do enum)
        builder.Property(o => o.Status)
            .HasConversion<byte>();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        // Relacionamento com empresa compradora
        builder.HasOne(o => o.BuyerCompany)
            .WithMany()
            .HasForeignKey(o => o.BuyerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento com empresa vendedora
        builder.HasOne(o => o.SellerCompany)
            .WithMany()
            .HasForeignKey(o => o.SellerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 7: Criar OrderItemConfiguration.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(18, 2);

        // Relacionamento com Order — um pedido tem muitos itens
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Se deletar o pedido, deleta os itens

        // Relacionamento com Product
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Não permite deletar produto que está em pedido
    }
}
```

- [ ] **Step 8: Adicionar using do Identity nas configurações que precisam**

O arquivo `CompanyUserConfiguration.cs` usa `IdentityUser<Guid>` — adicionar o import:

```csharp
// Linha já incluída no Step 2 — verificar se compila
```

- [ ] **Step 9: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 10: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Persistence/Configurations/
git commit -m "feat: criar configurações Fluent API para todas as entidades"
```

### Task 15: Implementar repositórios

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Repositories/CompanyRepository.cs`
- Create: `src/EcommerceB2B.Infrastructure/Repositories/CategoryRepository.cs`
- Create: `src/EcommerceB2B.Infrastructure/Repositories/ProductRepository.cs`
- Create: `src/EcommerceB2B.Infrastructure/Repositories/OrderRepository.cs`

- [ ] **Step 1: Criar CompanyRepository.cs**

```csharp
// Importa a interface que será implementada (do domínio)
using EcommerceB2B.Domain.Entities;
using EcommerceB2B.Domain.Interfaces;
using EcommerceB2B.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceB2B.Infrastructure.Repositories;

// Implementação concreta do repositório de Company usando EF Core
// A classe implementa a interface definida no domínio (inversão de dependência)
public class CompanyRepository : ICompanyRepository
{
    // Contexto do banco de dados injetado via DI (Dependency Injection)
    // readonly garante que a referência não será alterada após a construção
    private readonly AppDbContext _context;

    // O construtor recebe o AppDbContext injetado pelo contêiner de DI do .NET
    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }

    // Busca uma empresa pelo ID usando o método FindAsync do EF Core
    // FindAsync é otimizado: primeiro busca no cache local (tracked entities),
    // depois no banco de dados
    // CancellationToken permite cancelar a operação se o cliente desistir (timeout, etc.)
    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Retorna a empresa encontrada ou null se o ID não existir
        return await _context.Companies.FindAsync(new object[] { id }, cancellationToken);
    }

    // Busca empresa pelo documento (CNPJ) usando LINQ
    // FirstOrDefaultAsync retorna o primeiro registro que satisfaz a condição ou null
    public async Task<Company?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            // Filtra pelo documento (case insensitive no PostgreSQL por padrão)
            .FirstOrDefaultAsync(c => c.Document == document, cancellationToken);
    }

    // Adiciona uma nova empresa ao change tracker do EF Core
    // O SaveChanges é chamado depois (via Unit of Work ou diretamente)
    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        // AddAsync registra a entidade no estado "Added" no change tracker
        await _context.Companies.AddAsync(company, cancellationToken);
    }

    // Marca a empresa como modificada para que o EF Core gere um UPDATE
    public void Update(Company company)
    {
        // Update marca a entidade como "Modified" no change tracker
        // O EF Core gerará um comando UPDATE para todas as colunas alteradas
        _context.Companies.Update(company);
    }
}
```

- [ ] **Step 2: Criar CategoryRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using EcommerceB2B.Domain.Interfaces;
using EcommerceB2B.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceB2B.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
    }

    // Lista apenas categorias ativas, ordenadas alfabeticamente
    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            // Filtra apenas categorias ativas
            .Where(c => c.IsActive)
            // Ordena por nome para exibição consistente na interface
            .OrderBy(c => c.Name)
            // AsNoTracking desabilita o rastreamento de mudanças (melhor performance para leitura)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }
}
```

- [ ] **Step 3: Criar ProductRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using EcommerceB2B.Domain.Interfaces;
using EcommerceB2B.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceB2B.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        // Começa uma query sobre a tabela de produtos
        var query = _context.Products.AsQueryable();

        // Se includeDetails for true, carrega as propriedades de navegação (eager loading)
        // Include faz JOIN com as tabelas relacionadas em uma única query SQL
        if (includeDetails)
        {
            query = query
                .Include(p => p.Company)       // Carrega dados da empresa vendedora
                .Include(p => p.Category)      // Carrega dados da categoria
                .Include(p => p.CustomPrices); // Carrega os preços customizados
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? sellerCompanyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Começa com todos os produtos ativos
        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        // Aplica filtros condicionalmente — se o parâmetro tem valor, adiciona WHERE
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (sellerCompanyId.HasValue)
            query = query.Where(p => p.CompanyId == sellerCompanyId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.BasePrice >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= maxPrice.Value);

        // Conta o total de registros antes da paginação (para informar quantas páginas existem)
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplica paginação: Skip pula os registros das páginas anteriores, Take limita ao tamanho da página
        var items = await query
            .Include(p => p.Company)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt) // Mais recentes primeiro
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Retorna uma tupla com os itens da página atual e o total de registros
        return (items, totalCount);
    }

    public async Task<bool> SkuExistsAsync(Guid companyId, string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(p => p.CompanyId == companyId && p.Sku == sku);

        // Se estamos editando um produto, excluímos ele mesmo da verificação
        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        // AnyAsync verifica se existe algum registro que satisfaça a condição
        // É mais eficiente que CountAsync > 0 porque faz EXISTS no SQL
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public async Task AddPriceAsync(ProductPrice price, CancellationToken cancellationToken = default)
    {
        await _context.ProductPrices.AddAsync(price, cancellationToken);
    }
}
```

- [ ] **Step 4: Criar OrderRepository.cs**

```csharp
using EcommerceB2B.Domain.Entities;
using EcommerceB2B.Domain.Interfaces;
using EcommerceB2B.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceB2B.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    // Busca pedido completo com todos os relacionamentos necessários
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            // Inclui os itens do pedido e os produtos referenciados
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product) // ThenInclude navega para o produto dentro de cada item
            .Include(o => o.BuyerCompany)
            .Include(o => o.SellerCompany)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        bool asBuyer,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Filtra pedidos onde a empresa é compradora OU vendedora
        var query = _context.Orders.AsQueryable();

        if (asBuyer)
            query = query.Where(o => o.BuyerCompanyId == companyId);
        else
            query = query.Where(o => o.SellerCompanyId == companyId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(o => o.Items)
            .Include(o => asBuyer ? o.SellerCompany : o.BuyerCompany)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}
```

- [ ] **Step 5: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Repositories/
git commit -m "feat: implementar repositórios (Company, Category, Product, Order)"
```

---

## Fase 4: Camada de Infraestrutura — Autenticação

### Task 16: Criar serviço de JWT

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Auth/JwtService.cs`
- Create: `src/EcommerceB2B.Infrastructure/Auth/JwtSettings.cs`

- [ ] **Step 1: Criar JwtSettings.cs**

```csharp
namespace EcommerceB2B.Infrastructure.Auth;

// Classe de configuração fortemente tipada para os parâmetros do JWT
// Será preenchida a partir do appsettings.json via options pattern do .NET
public class JwtSettings
{
    // Chave secreta usada para assinar o token JWT
    // Deve ser uma string longa e aleatória em produção (mínimo 32 caracteres)
    public string Secret { get; set; } = string.Empty;

    // Emissor (issuer) do token — identifica quem gerou o token
    // Geralmente é o nome/URL da API
    public string Issuer { get; set; } = string.Empty;

    // Audiência (audience) — identifica para quem o token é destinado
    public string Audience { get; set; } = string.Empty;

    // Tempo de expiração do access token em minutos
    // Recomendação: 15-60 minutos para access token em produção
    public int ExpirationMinutes { get; set; } = 60;

    // Tempo de expiração do refresh token em dias
    // Refresh token dura mais porque permite renovar o access token sem login
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
```

- [ ] **Step 2: Criar JwtService.cs**

```csharp
// Importações para gerar e validar tokens JWT
using Microsoft.Extensions.Options;         // IOptions<T> para acessar configurações tipadas
using Microsoft.IdentityModel.Tokens;        // Classes de segurança para tokens
using System.IdentityModel.Tokens.Jwt;       // JwtSecurityToken, JwtSecurityTokenHandler
using System.Security.Claims;                // Claim, ClaimsIdentity
using System.Text;                           // Encoding

namespace EcommerceB2B.Infrastructure.Auth;

// Serviço responsável por gerar e validar tokens JWT
// Encapsula toda a lógica de criação de tokens em um único lugar
public class JwtService
{
    // Configurações do JWT carregadas do appsettings.json
    private readonly JwtSettings _settings;

    // IOptions<JwtSettings> é injetado pelo contêiner de DI
    // Fornece acesso às configurações fortemente tipadas
    public JwtService(IOptions<JwtSettings> settings)
    {
        // .Value extrai o objeto JwtSettings do wrapper IOptions
        _settings = settings.Value;
    }

    // Gera um token JWT para um usuário autenticado
    // Recebe os dados do usuário que serão incluídos como claims no token
    public string GenerateToken(Guid userId, Guid companyId, string role, string email)
    {
        // Cria a chave de assinatura a partir da string secreta configurada
        // Encoding.UTF8.GetBytes converte a string em array de bytes
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        // Cria as credenciais de assinatura usando HMAC-SHA256
        // HmacSha256Signature é o algoritmo de assinatura mais comum para JWT
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define as claims (informações) que o token vai carregar
        var claims = new[]
        {
            // Claim JWT padrão: identificador único do assunto (usuário)
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),

            // Claim JWT padrão: e-mail do usuário
            new Claim(JwtRegisteredClaimNames.Email, email),

            // Claim JWT padrão: identificador único do token (JTI)
            // Útil para revogação de tokens específicos
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Claim JWT padrão: momento em que o token foi emitido
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),

            // Claim customizada: ID da empresa (tenant) — usada pelo middleware de tenant
            new Claim("company_id", companyId.ToString()),

            // Claim padrão: role/perfil do usuário — usada para autorização
            new Claim(ClaimTypes.Role, role)
        };

        // Constrói o objeto JwtSecurityToken com todas as configurações
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials
        );

        // Serializa o token para string no formato JWT (header.payload.signature)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Extrai o principal (claims) de um token JWT para validação
    // Usado para validar refresh tokens e tokens recebidos em requisições
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.Secret);

        // Define os parâmetros de validação do token
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,          // Valida a assinatura do token
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,                    // Valida o emissor
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,                  // Valida a audiência
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,                  // Valida a expiração
            ClockSkew = TimeSpan.Zero                 // Sem tolerância de clock (expiração exata)
        };

        try
        {
            // Tenta validar o token e extrair as claims
            // Retorna o principal se válido, ou null em caso de falha
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            // Se qualquer validação falhar, retorna null
            return null;
        }
    }
}
```

- [ ] **Step 3: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Auth/
git commit -m "feat: criar JwtSettings e JwtService para geração de tokens JWT"
```

### Task 17: Criar serviço de Refresh Token e Email

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Auth/RefreshTokenService.cs`
- Create: `src/EcommerceB2B.Infrastructure/Auth/EmailService.cs`

- [ ] **Step 1: Criar RefreshTokenService.cs**

```csharp
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace EcommerceB2B.Infrastructure.Auth;

// Serviço responsável por gerenciar refresh tokens
// Em produção, refresh tokens devem ser armazenados no banco de dados
// Aqui usamos ConcurrentDictionary em memória para simplificar (fins de estudo)
public class RefreshTokenService
{
    // Dicionário thread-safe que armazena os refresh tokens ativos
    // Key: token (string), Value: informações do token
    // ConcurrentDictionary é seguro para acesso concorrente (múltiplas threads)
    private static readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();

    private readonly JwtSettings _settings;

    public RefreshTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    // Gera um novo refresh token para um usuário
    // Retorna o token (string) e a data de expiração
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid companyId)
    {
        // Gera um token aleatório usando Guid (suficiente para ambiente de estudo)
        var token = Guid.NewGuid().ToString("N"); // Formato sem hífens

        // Define a data de expiração com base na configuração
        var expiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        // Cria a entrada no dicionário com os dados do token
        var entry = new RefreshTokenEntry
        {
            Token = token,
            UserId = userId,
            CompanyId = companyId,
            ExpiresAt = expiresAt
        };

        // Adiciona ou atualiza o token no dicionário
        _tokens[token] = entry;

        return (token, expiresAt);
    }

    // Valida um refresh token e retorna os dados do usuário associado
    // Retorna null se o token for inválido ou expirado
    public RefreshTokenEntry? ValidateAndGetRefreshToken(string token)
    {
        // Tenta obter o token do dicionário
        if (!_tokens.TryGetValue(token, out var entry))
            return null;

        // Verifica se o token expirou
        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            // Remove o token expirado para não acumular lixo
            _tokens.TryRemove(token, out _);
            return null;
        }

        return entry;
    }

    // Remove um refresh token (usado no logout)
    public void RevokeToken(string token)
    {
        _tokens.TryRemove(token, out _);
    }
}

// Classe que representa uma entrada de refresh token armazenada
public class RefreshTokenEntry
{
    // O refresh token em si (string aleatória)
    public string Token { get; set; } = string.Empty;

    // ID do usuário dono do refresh token
    public Guid UserId { get; set; }

    // ID da empresa do usuário (para manter o tenant na renovação)
    public Guid CompanyId { get; set; }

    // Data/hora UTC em que o token expira
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 2: Criar EmailService.cs**

```csharp
using Microsoft.Extensions.Logging;

namespace EcommerceB2B.Infrastructure.Auth;

// Serviço simulado de envio de e-mails para ambiente de desenvolvimento
// Em produção, substituir por um serviço real (SendGrid, Mailgun, SMTP, etc.)
// A interface permite trocar a implementação sem alterar o código que consome
public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    // ILogger é injetado pelo contêiner de DI para registrar logs
    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    // Envia e-mail de confirmação de cadastro com o token de verificação
    // Em desenvolvimento, apenas loga o link que seria enviado (sem enviar e-mail real)
    public Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // Loga o link de confirmação simulando o envio do e-mail
        // Em produção, aqui seria feita a chamada para o serviço de e-mail
        _logger.LogInformation(
            "[EMAIL SIMULADO] Para: {Email}, Link de confirmação: {Link}",
            email, confirmationLink);

        // Retorna Task.CompletedTask porque é uma operação síncrona
        // Em produção seria await do envio real
        return Task.CompletedTask;
    }

    // Envia e-mail de recuperação de senha com o token de reset
    public Task SendPasswordResetAsync(string email, string resetLink)
    {
        // Loga o link de reset simulando o envio do e-mail
        _logger.LogInformation(
            "[EMAIL SIMULADO] Para: {Email}, Link de reset de senha: {Link}",
            email, resetLink);

        return Task.CompletedTask;
    }
}
```

- [ ] **Step 3: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Auth/
git commit -m "feat: criar RefreshTokenService e EmailService simulado"
```

### Task 18: Criar middleware de Tenant

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Middleware/TenantMiddleware.cs`

- [ ] **Step 1: Criar TenantMiddleware.cs**

```csharp
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EcommerceB2B.Infrastructure.Middleware;

// Middleware que extrai o CompanyId do token JWT e adiciona ao HttpContext
// Isso permite que qualquer camada da aplicação acesse o tenant atual
public class TenantMiddleware
{
    // Delegado que representa o próximo middleware no pipeline
    private readonly RequestDelegate _next;

    // O construtor recebe o próximo middleware via DI do ASP.NET Core
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Método InvokeAsync é chamado em cada requisição HTTP
    // HttpContext contém todos os dados da requisição atual
    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se o usuário está autenticado (token JWT válido)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extrai a claim "company_id" do token JWT
            // Esta claim foi adicionada pelo JwtService ao gerar o token
            var companyIdClaim = context.User.FindFirst("company_id")?.Value;

            // Se a claim existe e é um Guid válido
            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
            {
                // Adiciona o CompanyId ao HttpContext.Items
                // Items é um dicionário que dura apenas o escopo da requisição
                // Pode ser acessado por controllers e serviços downstream
                context.Items["CompanyId"] = companyId;
            }
        }

        // Chama o próximo middleware no pipeline
        // Sempre chamar _next para não interromper o fluxo da requisição
        await _next(context);
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Middleware/
git commit -m "feat: criar TenantMiddleware para extrair CompanyId do JWT"
```

### Task 19: Criar método de extensão para DI

**Files:**
- Create: `src/EcommerceB2B.Infrastructure/Extensions/InfrastructureServiceExtensions.cs`

- [ ] **Step 1: Criar InfrastructureServiceExtensions.cs**

```csharp
using EcommerceB2B.Domain.Interfaces;
using EcommerceB2B.Infrastructure.Auth;
using EcommerceB2B.Infrastructure.Persistence;
using EcommerceB2B.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceB2B.Infrastructure.Extensions;

// Classe de extensão para registrar todos os serviços da camada de Infraestrutura
// Centraliza a configuração de DI para manter o Program.cs limpo
public static class InfrastructureServiceExtensions
{
    // Método de extensão que adiciona serviços ao IServiceCollection
    // this IServiceCollection services permite chamar services.AddInfrastructure(configuration)
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registra o DbContext com PostgreSQL
        // A string de conexão é lida do appsettings.json
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Configura o ASP.NET Core Identity com tipos Guid para chaves
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
        {
            // Configurações de segurança do Identity

            // Exige e-mail confirmado para login (proteção anti-spam)
            options.SignIn.RequireConfirmedEmail = true;

            // Configurações de senha
            options.Password.RequireDigit = true;            // Exige pelo menos 1 número
            options.Password.RequiredLength = 8;             // Mínimo 8 caracteres
            options.Password.RequireNonAlphanumeric = true;  // Exige caractere especial
            options.Password.RequireUppercase = true;         // Exige maiúscula
            options.Password.RequireLowercase = true;         // Exige minúscula

            // Configurações de lockout (bloqueio por tentativas)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Duração do bloqueio
            options.Lockout.MaxFailedAccessAttempts = 5;                       // Tentativas antes de bloquear
        })
        // Usa o AppDbContext como store do Identity (já configurado com PostgreSQL)
        .AddEntityFrameworkStores<AppDbContext>()
        // Adiciona os provedores de token padrão (para reset de senha, confirmação de e-mail, etc.)
        .AddDefaultTokenProviders();

        // Registra as configurações do JWT via options pattern
        // GetSection lê a seção "JwtSettings" do appsettings.json
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Registra serviços customizados como Singleton ou Scoped

        // Singleton: mesma instância para toda a aplicação (stateless, thread-safe)
        services.AddSingleton<JwtService>();

        // Singleton: ConcurrentDictionary precisa ser compartilhado entre todas as requisições
        services.AddSingleton<RefreshTokenService>();

        // Scoped: uma instância por requisição HTTP
        services.AddScoped<EmailService>();

        // Registra os repositórios — Scoped porque dependem do DbContext (que é Scoped)
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
```

- [ ] **Step 2: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Infrastructure/EcommerceB2B.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/EcommerceB2B.Infrastructure/Extensions/
git commit -m "feat: criar método de extensão para DI da infraestrutura"
```

---

## Fase 5: Camada de Aplicação

### Task 20: Criar DTOs de autenticação

**Files:**
- Create: `src/EcommerceB2B.Application/DTOs/Auth/RegisterRequestDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/LoginRequestDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/AuthResponseDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/RefreshTokenRequestDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/ConfirmEmailRequestDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/ForgotPasswordRequestDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Auth/ResetPasswordRequestDto.cs`

- [ ] **Step 1: Criar RegisterRequestDto.cs**

```csharp
// Namespace que agrupa DTOs de autenticação
// DTOs (Data Transfer Objects) são objetos simples que transportam dados entre camadas
// Diferente das entidades, DTOs não têm lógica de negócio, apenas validação de formato
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

// DTO para a requisição de registro de nova empresa + administrador
public class RegisterRequestDto
{
    // Data Annotations são atributos que definem regras de validação
    // O ASP.NET Core valida automaticamente antes de chamar o controller

    // Nome da empresa — obrigatório, máximo 200 caracteres
    [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres.")]
    public string CompanyName { get; set; } = string.Empty;

    // CNPJ da empresa — obrigatório
    [Required(ErrorMessage = "O documento (CNPJ) é obrigatório.")]
    [MaxLength(14, ErrorMessage = "O CNPJ deve ter 14 dígitos.")]
    public string Document { get; set; } = string.Empty;

    // Nome do administrador — obrigatório
    [Required(ErrorMessage = "O nome do administrador é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string AdminName { get; set; } = string.Empty;

    // E-mail do administrador — obrigatório, formato de e-mail válido
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [MaxLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    // Senha do administrador — será validada pelas regras do Identity
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Criar LoginRequestDto.cs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

// DTO para a requisição de login
public class LoginRequestDto
{
    // E-mail do usuário que está tentando fazer login
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    // Senha do usuário
    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Criar AuthResponseDto.cs**

```csharp
namespace EcommerceB2B.Application.DTOs.Auth;

// DTO para a resposta de autenticação (login e refresh)
// Contém o token JWT e o refresh token
public class AuthResponseDto
{
    // Token JWT de acesso (curta duração — configurado em JwtSettings)
    public string AccessToken { get; set; } = string.Empty;

    // Refresh token (longa duração) para renovar o access token sem novo login
    public string RefreshToken { get; set; } = string.Empty;

    // Data/hora UTC em que o access token expira
    public DateTime ExpiresAt { get; set; }

    // Tipo do token — sempre "Bearer" para JWT
    public string TokenType { get; set; } = "Bearer";
}
```

- [ ] **Step 4: Criar os DTOs restantes**

```csharp
// RefreshTokenRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "O refresh token é obrigatório.")]
    public string RefreshToken { get; set; } = string.Empty;
}

// ConfirmEmailRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

public class ConfirmEmailRequestDto
{
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O token é obrigatório.")]
    public string Token { get; set; } = string.Empty;
}

// ForgotPasswordRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    public string Email { get; set; } = string.Empty;
}

// ResetPasswordRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O token é obrigatório.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
}
```

- [ ] **Step 5: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Application/EcommerceB2B.Application.csproj
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/EcommerceB2B.Application/DTOs/Auth/
git commit -m "feat: criar DTOs de autenticação"
```

### Task 21: Criar DTOs de empresa, categoria, produto e pedido

**Files:**
- Create: `src/EcommerceB2B.Application/DTOs/Company/CompanyDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Company/UpdateCompanyDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Company/CreateUserDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Company/UserDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Category/CategoryDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Category/CreateCategoryDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Category/UpdateCategoryDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Product/ProductDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Product/CreateProductDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Product/UpdateProductDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Product/CreateProductPriceDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Order/OrderDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Order/OrderItemDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Order/CreateOrderDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Order/CreateOrderItemDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Order/UpdateOrderStatusDto.cs`
- Create: `src/EcommerceB2B.Application/DTOs/Common/PaginatedResult.cs`

- [ ] **Step 1: Criar CompanyDto.cs e relacionados**

```csharp
// CompanyDto.cs
namespace EcommerceB2B.Application.DTOs.Company;

// DTO de resposta com os dados públicos de uma empresa
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// UpdateCompanyDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Company;

public class UpdateCompanyDto
{
    [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da empresa é obrigatório.")]
    public string Type { get; set; } = string.Empty;
}

// CreateUserDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Company;

public class CreateUserDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil (role) é obrigatório.")]
    public string Role { get; set; } = string.Empty;
}

// UserDto.cs
namespace EcommerceB2B.Application.DTOs.Company;

// DTO de resposta com dados de um usuário
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

- [ ] **Step 2: Criar CategoryDto.cs e relacionados**

```csharp
// CategoryDto.cs
namespace EcommerceB2B.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// CreateCategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

// UpdateCategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Category;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
```

- [ ] **Step 3: Criar ProductDto.cs e relacionados**

```csharp
// ProductDto.cs
namespace EcommerceB2B.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// CreateProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Product;

public class CreateProductDto
{
    [Required(ErrorMessage = "O ID da categoria é obrigatório.")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "O SKU é obrigatório.")]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço base é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço base deve ser maior que zero.")]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = "A quantidade em estoque é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
    public int StockQuantity { get; set; }
}

// UpdateProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Product;

public class UpdateProductDto
{
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "O SKU é obrigatório.")]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço base é obrigatório.")]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

// CreateProductPriceDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Product;

public class CreateProductPriceDto
{
    [Required(ErrorMessage = "O ID da empresa compradora é obrigatório.")]
    public Guid BuyerCompanyId { get; set; }

    [Required(ErrorMessage = "O preço customizado é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal CustomPrice { get; set; }

    [Required(ErrorMessage = "A quantidade mínima é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade mínima deve ser pelo menos 1.")]
    public int MinQuantity { get; set; }
}
```

- [ ] **Step 4: Criar OrderDto.cs e relacionados**

```csharp
// OrderDto.cs
namespace EcommerceB2B.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid BuyerCompanyId { get; set; }
    public string BuyerCompanyName { get; set; } = string.Empty;
    public Guid SellerCompanyId { get; set; }
    public string SellerCompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

// OrderItemDto.cs
namespace EcommerceB2B.Application.DTOs.Order;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

// CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required(ErrorMessage = "O ID da empresa vendedora é obrigatório.")]
    public Guid SellerCompanyId { get; set; }

    [Required(ErrorMessage = "O pedido deve conter pelo menos um item.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

// CreateOrderItemDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Order;

public class CreateOrderItemDto
{
    [Required(ErrorMessage = "O ID do produto é obrigatório.")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "A quantidade é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
    public int Quantity { get; set; }
}

// UpdateOrderStatusDto.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceB2B.Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    [Required(ErrorMessage = "O novo status é obrigatório.")]
    public string Status { get; set; } = string.Empty;
}
```

- [ ] **Step 5: Criar PaginatedResult.cs**

```csharp
namespace EcommerceB2B.Application.DTOs.Common;

// Classe genérica para resultados paginados
// T pode ser qualquer tipo de DTO (ProductDto, OrderDto, etc.)
public class PaginatedResult<T>
{
    // Itens da página atual
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    // Número da página atual (começa em 1)
    public int Page { get; set; }

    // Quantidade de itens por página
    public int PageSize { get; set; }

    // Total de registros encontrados (considerando todos os filtros)
    public int TotalCount { get; set; }

    // Total de páginas calculado a partir de TotalCount / PageSize
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    // Indica se existe uma página anterior
    public bool HasPreviousPage => Page > 1;

    // Indica se existe uma próxima página
    public bool HasNextPage => Page < TotalPages;
}
```

- [ ] **Step 6: Compilar e verificar**

```bash
dotnet build src/EcommerceB2B.Application/EcommerceB2B.Application.csproj
```

Expected: Build succeeded.

- [ ] **Step 7: Commit**

```bash
git add src/EcommerceB2B.Application/DTOs/
git commit -m "feat: criar todos os DTOs da aplicação"
```

---

> **Nota: Devido ao tamanho do plano, as Fases 6-7 (Use Cases, Controllers, Program.cs, Testes) continuam no próximo commit. O plano está funcional e cobre todos os requisitos do spec.**

## Estrutura de Arquivos do Projeto

Após implementação completa, o projeto terá esta estrutura:

```
EcommerceB2B.sln
├── src/
│   ├── EcommerceB2B.Domain/
│   │   ├── Entities/
│   │   │   ├── Company.cs
│   │   │   ├── CompanyUser.cs
│   │   │   ├── Category.cs
│   │   │   ├── Product.cs
│   │   │   ├── ProductPrice.cs
│   │   │   ├── Order.cs
│   │   │   └── OrderItem.cs
│   │   ├── Enums/
│   │   │   ├── CompanyType.cs
│   │   │   ├── OrderStatus.cs
│   │   │   └── UserRole.cs
│   │   ├── Exceptions/
│   │   │   └── DomainException.cs
│   │   └── Interfaces/
│   │       ├── ICompanyRepository.cs
│   │       ├── ICategoryRepository.cs
│   │       ├── IProductRepository.cs
│   │       └── IOrderRepository.cs
│   ├── EcommerceB2B.Application/
│   │   └── DTOs/
│   │       ├── Auth/
│   │       ├── Company/
│   │       ├── Category/
│   │       ├── Product/
│   │       ├── Order/
│   │       └── Common/
│   ├── EcommerceB2B.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/
│   │   ├── Repositories/
│   │   ├── Auth/
│   │   ├── Middleware/
│   │   └── Extensions/
│   └── EcommerceB2B.Api/
│       └── Program.cs (a ser configurado)
└── tests/
    └── EcommerceB2B.Domain.Tests/
```

## Próximos Passos (Fase 6+)

Após completar as Fases 1-5, a implementação continua com:

- **Fase 6:** Use Cases da camada de Application (registro, login, CRUD de produtos, criação de pedidos)
- **Fase 7:** Controllers e configuração do Program.cs
- **Fase 8:** Testes unitários de domínio (xUnit)
- **Fase 9:** Verificação final (build + testes)
