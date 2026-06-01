// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// A classe Product representa um produto no catálogo B2B da plataforma
// Cada produto pertence a uma empresa (vendedor) e a uma categoria
// Contém informações de preço, estoque e SKU para identificação
public class Product
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    private Product()
    {
    }

    // Construtor público: cria um novo produto com todos os dados obrigatórios
    // A descrição é opcional, todos os demais campos são obrigatórios
    public Product(Guid companyId, Guid categoryId, string name, string sku, decimal basePrice, int stockQuantity, string? description = null)
    {
        // Valida que o ID da empresa não é vazio (deve referenciar uma empresa existente)
        if (companyId == Guid.Empty)
        {
            // Lança exceção informando que o ID da empresa é obrigatório
            throw new DomainException("O ID da empresa é obrigatório.");
        }

        // Valida que o ID da categoria não é vazio (deve referenciar uma categoria existente)
        if (categoryId == Guid.Empty)
        {
            // Lança exceção informando que o ID da categoria é obrigatório
            throw new DomainException("O ID da categoria é obrigatório.");
        }

        // Gera um identificador único para o produto
        Id = Guid.NewGuid();

        // Atribui os identificadores validados
        CompanyId = companyId;
        CategoryId = categoryId;

        // Valida e define os atributos obrigatórios usando os métodos setter
        SetName(name);
        SetSku(sku);
        SetBasePrice(basePrice);
        SetStockQuantity(stockQuantity);

        // Define a descrição opcional (pode ser nula)
        Description = description;

        // Todo produto novo é criado como ativo
        IsActive = true;

        // Registra a data/hora de criação em UTC
        CreatedAt = DateTime.UtcNow;
    }

    // Identificador único do produto
    public Guid Id { get; private set; }

    // Chave estrangeira: ID da empresa vendedora do produto
    // Relaciona o produto ao seu fornecedor na plataforma
    public Guid CompanyId { get; private set; }

    // Chave estrangeira: ID da categoria à qual o produto pertence
    // Permite organizar produtos em grupos para navegação e busca
    public Guid CategoryId { get; private set; }

    // Nome do produto como aparece no catálogo (ex: "Notebook Dell XPS 15")
    public string Name { get; private set; } = null!;

    // Descrição detalhada opcional do produto (especificações, características)
    public string? Description { get; private set; }

    // SKU (Stock Keeping Unit): código único de identificação do produto no estoque
    // Essencial para controle de inventário e localização rápida do produto
    public string Sku { get; private set; } = null!;

    // Preço base do produto (preço de tabela para todos os clientes)
    // decimal é usado para valores monetários por sua precisão (evita erros de arredondamento)
    public decimal BasePrice { get; private set; }

    // Quantidade atual em estoque do produto
    // Permite controle de disponibilidade e evita vendas sem estoque
    public int StockQuantity { get; private set; }

    // Indica se o produto está ativo e disponível para venda
    public bool IsActive { get; private set; }

    // Data e hora de criação do registro (UTC) para auditoria
    public DateTime CreatedAt { get; private set; }

    // Propriedade de navegação: empresa vendedora dona do produto
    // virtual permite lazy loading pelo EF Core
    public virtual Company Company { get; private set; } = null!;

    // Propriedade de navegação: categoria à qual o produto pertence
    public virtual Category Category { get; private set; } = null!;

    // Propriedade de navegação: coleção de preços customizados para clientes específicos
    // Permite que cada comprador tenha um preço especial negociado
    // HashSet garante unicidade e busca eficiente O(1)
    public virtual ICollection<ProductPrice> CustomPrices { get; private set; } = new HashSet<ProductPrice>();

    // Atualiza o nome do produto com validação
    public void SetName(string name)
    {
        // Valida que o nome não é nulo, vazio ou apenas espaços
        if (string.IsNullOrWhiteSpace(name))
        {
            // Lança exceção informando que o nome é obrigatório
            throw new DomainException("O nome do produto é obrigatório.");
        }

        // Atribui o nome validado
        Name = name;
    }

    // Atualiza a descrição do produto (campo opcional)
    public void SetDescription(string? description)
    {
        // Atribui a descrição sem validação obrigatória
        // Aceita null para remover a descrição existente
        Description = description;
    }

    // Atualiza o SKU do produto com validação
    // O SKU é essencial para identificação única do produto no estoque
    public void SetSku(string sku)
    {
        // Valida que o SKU não é nulo, vazio ou apenas espaços
        if (string.IsNullOrWhiteSpace(sku))
        {
            // Lança exceção informando que o SKU é obrigatório
            throw new DomainException("O SKU do produto é obrigatório.");
        }

        // Atribui o SKU validado
        Sku = sku;
    }

    // Atualiza o preço base do produto com validação
    // O preço não pode ser negativo (representaria um valor inválido)
    public void SetBasePrice(decimal basePrice)
    {
        // Valida que o preço não é negativo
        // Preço igual a zero pode ser válido para produtos gratuitos (ex: amostras)
        if (basePrice < 0)
        {
            // Lança exceção informando que o preço não pode ser negativo
            throw new DomainException("O preço base do produto não pode ser negativo.");
        }

        // Atribui o preço validado
        BasePrice = basePrice;
    }

    // Atualiza a quantidade em estoque com validação
    // O estoque não pode ser negativo (representaria inconsistência)
    public void SetStockQuantity(int stockQuantity)
    {
        // Valida que a quantidade não é negativa
        // Zero é válido: significa produto esgotado
        if (stockQuantity < 0)
        {
            // Lança exceção informando que o estoque não pode ser negativo
            throw new DomainException("A quantidade em estoque não pode ser negativa.");
        }

        // Atribui a quantidade validada
        StockQuantity = stockQuantity;
    }

    // Altera a categoria do produto
    // Permite recategorizar produtos sem precisar recriá-los
    public void SetCategory(Guid categoryId)
    {
        // Valida que o ID da categoria não é vazio
        if (categoryId == Guid.Empty)
        {
            // Lança exceção informando que a categoria é obrigatória
            throw new DomainException("O ID da categoria é obrigatório.");
        }

        // Atribui o novo ID de categoria
        CategoryId = categoryId;
    }

    // Ativa o produto, disponibilizando-o para venda novamente
    public void Activate()
    {
        // Define a flag IsActive como true (ativo)
        IsActive = true;
    }

    // Desativa o produto, removendo-o do catálogo de vendas
    // Soft delete: o registro permanece no banco para histórico
    public void Deactivate()
    {
        // Define a flag IsActive como false (inativo)
        IsActive = false;
    }
}
