// Importa as entidades de dominio que serao testadas
using EcommerceB2B.Domain.Entities;

// Importa a excecao customizada que esperamos capturar nos testes de validacao
using EcommerceB2B.Domain.Exceptions;

// Namespace que agrupa os testes unitarios do dominio
namespace EcommerceB2B.Domain.Tests;

// Classe de testes para a entidade Product (produto)
// Testa construtor, metodos de validacao, atualizacao e desativacao
public class ProductTests
{
    // Dados reutilizaveis: IDs de empresa e categoria para criar produtos
    private readonly Guid _companyId = Guid.NewGuid();  // Empresa vendedora ficticia
    private readonly Guid _categoryId = Guid.NewGuid(); // Categoria ficticia

    // ─────────────────────────────────────────────────────────
    // Testes do Construtor do Produto
    // ─────────────────────────────────────────────────────────

    // Verifica que o construtor cria um produto com todos os campos validos
    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarProduto()
    {
        // Arrange
        var name = "Notebook Dell XPS 15";
        var sku = "DELL-XPS15-001";
        var basePrice = 7500m;
        var stockQuantity = 10;
        var description = "Notebook de alta performance com 16GB RAM";

        // Act: cria o produto
        var product = new Product(_companyId, _categoryId, name, sku, basePrice, stockQuantity, description);

        // Assert: verifica que todos os campos foram preenchidos corretamente
        Assert.NotEqual(Guid.Empty, product.Id);          // ID unico gerado
        Assert.Equal(_companyId, product.CompanyId);      // Empresa dona correta
        Assert.Equal(_categoryId, product.CategoryId);    // Categoria correta
        Assert.Equal(name, product.Name);                 // Nome do produto
        Assert.Equal(sku, product.Sku);                   // SKU correto
        Assert.Equal(basePrice, product.BasePrice);       // Preco base
        Assert.Equal(stockQuantity, product.StockQuantity); // Quantidade em estoque
        Assert.Equal(description, product.Description);   // Descricao
        Assert.True(product.IsActive);                    // Produto novo deve estar ativo
        Assert.True(product.CreatedAt > DateTime.MinValue); // Data de criacao preenchida
    }

    // Verifica que a descricao e opcional (pode ser nula)
    [Fact]
    public void Constructor_SemDescricao_DeveCriarProdutoComDescricaoNula()
    {
        // Arrange: sem descricao
        var name = "Produto Basico";
        var sku = "BAS-001";

        // Act: cria sem passar descricao (usa valor padrao null)
        var product = new Product(_companyId, _categoryId, name, sku, 50m, 5);

        // Assert: descricao deve ser null
        Assert.Null(product.Description);
        Assert.Equal(name, product.Name);
        Assert.True(product.IsActive);
    }

    // ─────────────────────────────────────────────────────────
    // Testes de Validacao do Construtor
    // ─────────────────────────────────────────────────────────

    // Verifica que o ID da empresa nao pode ser Guid.Empty
    [Fact]
    public void Constructor_ComCompanyIdVazio_DeveLancarDomainException()
    {
        // Act e Assert: tenta criar produto com Guid.Empty como empresa
        var exception = Assert.Throws<DomainException>(() =>
            new Product(Guid.Empty, _categoryId, "Produto", "SKU001", 100m, 10));

        Assert.Contains("empresa", exception.Message.ToLower());
    }

    // Verifica que o ID da categoria nao pode ser Guid.Empty
    [Fact]
    public void Constructor_ComCategoryIdVazio_DeveLancarDomainException()
    {
        // Act e Assert: tenta criar produto com Guid.Empty como categoria
        var exception = Assert.Throws<DomainException>(() =>
            new Product(_companyId, Guid.Empty, "Produto", "SKU001", 100m, 10));

        Assert.Contains("categoria", exception.Message.ToLower());
    }

    // Verifica que o nome nao pode ser vazio
    [Fact]
    public void Constructor_ComNomeVazio_DeveLancarDomainException()
    {
        // Act e Assert: nome vazio nao e permitido
        var exception = Assert.Throws<DomainException>(() =>
            new Product(_companyId, _categoryId, "", "SKU001", 100m, 10));

        Assert.Contains("nome", exception.Message.ToLower());
    }

    // Verifica que o SKU nao pode ser vazio
    [Fact]
    public void Constructor_ComSkuVazio_DeveLancarDomainException()
    {
        // Act e Assert: SKU vazio nao e permitido (essencial para identificacao)
        var exception = Assert.Throws<DomainException>(() =>
            new Product(_companyId, _categoryId, "Produto", "", 100m, 10));

        Assert.Contains("SKU", exception.Message);
    }

    // Verifica que o preco base nao pode ser negativo
    [Fact]
    public void Constructor_ComPrecoBaseNegativo_DeveLancarDomainException()
    {
        // Act e Assert: preco negativo nao faz sentido comercialmente
        var exception = Assert.Throws<DomainException>(() =>
            new Product(_companyId, _categoryId, "Produto", "SKU001", -10m, 10));

        Assert.Contains("preço", exception.Message.ToLower());
    }

    // Verifica que o preco base pode ser zero (produto gratuito, amostras)
    [Fact]
    public void Constructor_ComPrecoBaseZero_DeveCriarProduto()
    {
        // Act: cria produto com preco zero (valido — ex: amostra gratis)
        var product = new Product(_companyId, _categoryId, "Amostra Gratis", "AMO-001", 0m, 100);

        // Assert: preco zero e aceito
        Assert.Equal(0m, product.BasePrice);
        Assert.True(product.IsActive);
    }

    // Verifica que estoque nao pode ser negativo
    [Fact]
    public void Constructor_ComEstoqueNegativo_DeveLancarDomainException()
    {
        // Act e Assert: estoque negativo representa inconsistencia
        var exception = Assert.Throws<DomainException>(() =>
            new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, -5));

        Assert.Contains("estoque", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes dos Metodos de Atualizacao (Setters)
    // ─────────────────────────────────────────────────────────

    // Verifica que SetName atualiza o nome com valor valido
    [Fact]
    public void SetName_ComNomeValido_DeveAtualizarNome()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Nome Antigo", "SKU001", 100m, 10);

        // Act
        product.SetName("Nome Novo");

        // Assert
        Assert.Equal("Nome Novo", product.Name);
    }

    // Verifica que SetName rejeita nome vazio
    [Fact]
    public void SetName_ComNomeVazio_DeveLancarDomainException()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act e Assert
        var exception = Assert.Throws<DomainException>(() => product.SetName(""));
        Assert.Contains("nome", exception.Message.ToLower());
    }

    // Verifica que SetSku atualiza o SKU com valor valido
    [Fact]
    public void SetSku_ComSkuValido_DeveAtualizarSku()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU-ANTIGO", 100m, 10);

        // Act
        product.SetSku("SKU-NOVO");

        // Assert
        Assert.Equal("SKU-NOVO", product.Sku);
    }

    // Verifica que SetSku rejeita SKU vazio
    [Fact]
    public void SetSku_ComSkuVazio_DeveLancarDomainException()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act e Assert
        var exception = Assert.Throws<DomainException>(() => product.SetSku(""));
        Assert.Contains("SKU", exception.Message);
    }

    // Verifica que SetBasePrice atualiza o preco com valor valido
    [Fact]
    public void SetBasePrice_ComPrecoValido_DeveAtualizarPreco()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act
        product.SetBasePrice(199.90m);

        // Assert
        Assert.Equal(199.90m, product.BasePrice);
    }

    // Verifica que SetBasePrice rejeita preco negativo
    [Fact]
    public void SetBasePrice_ComPrecoNegativo_DeveLancarDomainException()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act e Assert
        var exception = Assert.Throws<DomainException>(() => product.SetBasePrice(-1m));
        Assert.Contains("preço", exception.Message.ToLower());
    }

    // Verifica que SetStockQuantity atualiza o estoque com valor valido
    [Fact]
    public void SetStockQuantity_ComQuantidadeValida_DeveAtualizarEstoque()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act: atualiza estoque para quantidade maior
        product.SetStockQuantity(50);

        // Assert
        Assert.Equal(50, product.StockQuantity);
    }

    // Verifica que estoque zero e valido (produto esgotado)
    [Fact]
    public void SetStockQuantity_ComZero_DeveAceitar()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto Esgotado", "SKU001", 100m, 100);

        // Act: zera o estoque
        product.SetStockQuantity(0);

        // Assert: estoque zero indica produto esgotado, nao inconsistencia
        Assert.Equal(0, product.StockQuantity);
    }

    // Verifica que SetStockQuantity rejeita estoque negativo
    [Fact]
    public void SetStockQuantity_ComNegativo_DeveLancarDomainException()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act e Assert
        var exception = Assert.Throws<DomainException>(() => product.SetStockQuantity(-1));
        Assert.Contains("estoque", exception.Message.ToLower());
    }

    // Verifica que SetCategory atualiza a categoria do produto
    [Fact]
    public void SetCategory_ComIdValido_DeveAtualizarCategoria()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);
        var novaCategoriaId = Guid.NewGuid();

        // Act: move o produto para outra categoria
        product.SetCategory(novaCategoriaId);

        // Assert
        Assert.Equal(novaCategoriaId, product.CategoryId);
    }

    // Verifica que SetCategory rejeita Guid.Empty
    [Fact]
    public void SetCategory_ComIdVazio_DeveLancarDomainException()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act e Assert
        var exception = Assert.Throws<DomainException>(() => product.SetCategory(Guid.Empty));
        Assert.Contains("categoria", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes dos Metodos Activate e Deactivate
    // ─────────────────────────────────────────────────────────

    // Verifica que Deactivate desativa o produto (soft delete)
    [Fact]
    public void Deactivate_DeveDefinirIsActiveComoFalso()
    {
        // Arrange: cria produto ativo
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act: desativa o produto
        product.Deactivate();

        // Assert: produto deve estar inativo
        Assert.False(product.IsActive);
    }

    // Verifica que Activate reativa um produto desativado
    [Fact]
    public void Activate_AposDeactivate_DeveReativarProduto()
    {
        // Arrange: cria e desativa o produto
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);
        product.Deactivate();

        // Act: reativa o produto
        product.Activate();

        // Assert: produto deve voltar a estar ativo
        Assert.True(product.IsActive);
    }

    // ─────────────────────────────────────────────────────────
    // Testes do Metodo SetDescription
    // ─────────────────────────────────────────────────────────

    // Verifica que SetDescription atualiza a descricao
    [Fact]
    public void SetDescription_ComDescricaoValida_DeveAtualizarDescricao()
    {
        // Arrange
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10);

        // Act
        product.SetDescription("Nova descricao detalhada do produto");

        // Assert
        Assert.Equal("Nova descricao detalhada do produto", product.Description);
    }

    // Verifica que SetDescription aceita null (remove descricao)
    [Fact]
    public void SetDescription_ComNull_DeveRemoverDescricao()
    {
        // Arrange: cria produto COM descricao
        var product = new Product(_companyId, _categoryId, "Produto", "SKU001", 100m, 10,
            "Descricao que sera removida");

        // Act: remove a descricao passando null
        product.SetDescription(null);

        // Assert: descricao deve ser null
        Assert.Null(product.Description);
    }
}
