// Define o namespace do DTO dentro da camada de Aplicacao, na area de Produto
namespace EcommerceB2B.Application.DTOs.Product;

/// <summary>
/// DTO que representa os dados completos de um produto para leitura (retorno em consultas).
/// Inclui informacoes da empresa fornecedora e categoria para facilitar a exibicao em tela.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Identificador unico global (GUID) do produto no sistema.
    /// </summary>
    public Guid Id { get; set; } // Identificador unico do produto (UUID)

    /// <summary>
    /// Identificador da empresa fornecedora (Supplier) dona do produto.
    /// Utilizado para relacionamentos e filtros no backend.
    /// </summary>
    public Guid CompanyId { get; set; } // ID da empresa fornecedora do produto

    /// <summary>
    /// Nome da empresa fornecedora do produto.
    /// Incluido para evitar chamadas adicionais ao buscar dados do produto.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty; // Nome da empresa fornecedora (desnormalizado para leitura)

    /// <summary>
    /// Identificador da categoria a qual o produto pertence.
    /// Utilizado para agrupamento e filtros de produtos.
    /// </summary>
    public Guid CategoryId { get; set; } // ID da categoria do produto

    /// <summary>
    /// Nome da categoria do produto.
    /// Exibido nas listagens e detalhes sem necessidade de consulta adicional.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty; // Nome da categoria (desnormalizado para leitura)

    /// <summary>
    /// Nome do produto conforme cadastrado pelo fornecedor.
    /// </summary>
    public string Name { get; set; } = string.Empty; // Nome comercial do produto

    /// <summary>
    /// Descricao detalhada do produto com caracteristicas e especificacoes.
    /// Campo opcional; pode ser nulo se o fornecedor nao cadastrou descricao.
    /// </summary>
    public string? Description { get; set; } // Descricao opcional do produto (pode ser nula)

    /// <summary>
    /// Codigo SKU (Stock Keeping Unit) unico do produto.
    /// Utilizado para controle de estoque e identificacao interna pelo fornecedor.
    /// </summary>
    public string Sku { get; set; } = string.Empty; // Codigo SKU do produto para controle de estoque

    /// <summary>
    /// Preco base (sugerido) do produto.
    /// Precos especificos por comprador sao definidos em tabela de precos separada.
    /// </summary>
    public decimal BasePrice { get; set; } // Preco base do produto (decimal para precisao financeira)

    /// <summary>
    /// Quantidade atual em estoque do produto.
    /// Controlado pelo fornecedor; afeta a disponibilidade para compra.
    /// </summary>
    public int StockQuantity { get; set; } // Quantidade disponivel em estoque

    /// <summary>
    /// Indica se o produto esta ativo e disponivel para compra.
    /// Produtos inativos nao aparecem nas buscas e nao podem ser adicionados a pedidos.
    /// </summary>
    public bool IsActive { get; set; } // Status de atividade do produto

    /// <summary>
    /// Data e hora (UTC) em que o produto foi cadastrado no sistema.
    /// </summary>
    public DateTime CreatedAt { get; set; } // Data de criacao do registro do produto
}
