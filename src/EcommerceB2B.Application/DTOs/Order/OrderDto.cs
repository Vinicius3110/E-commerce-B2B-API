// Define o namespace do DTO dentro da camada de Aplicacao, na area de Pedido
namespace EcommerceB2B.Application.DTOs.Order;

/// <summary>
/// DTO que representa os dados completos de um pedido para leitura (retorno em consultas).
/// Inclui informacoes do comprador, vendedor e a lista de itens do pedido.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Identificador unico global (GUID) do pedido no sistema.
    /// </summary>
    public Guid Id { get; set; } // Identificador unico do pedido (UUID)

    /// <summary>
    /// Identificador da empresa compradora (Buyer) que realizou o pedido.
    /// </summary>
    public Guid BuyerCompanyId { get; set; } // ID da empresa compradora

    /// <summary>
    /// Nome da empresa compradora.
    /// Incluido para exibicao direta sem necessidade de consulta adicional.
    /// </summary>
    public string BuyerCompanyName { get; set; } = string.Empty; // Nome da empresa compradora

    /// <summary>
    /// Identificador da empresa vendedora (Supplier) que recebeu o pedido.
    /// </summary>
    public Guid SellerCompanyId { get; set; } // ID da empresa vendedora/fornecedora

    /// <summary>
    /// Nome da empresa vendedora.
    /// Exibido nos detalhes do pedido para o comprador.
    /// </summary>
    public string SellerCompanyName { get; set; } = string.Empty; // Nome da empresa vendedora

    /// <summary>
    /// Status atual do pedido no fluxo de processamento.
    /// Exemplos: "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled".
    /// </summary>
    public string Status { get; set; } = string.Empty; // Status atual do pedido

    /// <summary>
    /// Valor total do pedido (soma dos totais de todos os itens).
    /// Calculado a partir dos precos unitarios e quantidades de cada item.
    /// </summary>
    public decimal TotalAmount { get; set; } // Valor total do pedido em decimal para precisao financeira

    /// <summary>
    /// Data e hora (UTC) em que o pedido foi criado.
    /// </summary>
    public DateTime CreatedAt { get; set; } // Data de criacao do pedido

    /// <summary>
    /// Lista de itens que compoe o pedido.
    /// Cada item representa um produto com sua quantidade, preco e subtotal.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new(); // Lista de itens do pedido (inicializada vazia para evitar null)
}
