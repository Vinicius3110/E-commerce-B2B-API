// Define o namespace do DTO dentro da camada de Aplicacao, na area de Pedido
namespace EcommerceB2B.Application.DTOs.Order;

/// <summary>
/// DTO que representa um item individual dentro de um pedido.
/// Cada item referencia um produto especifico com sua quantidade e precos.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Identificador unico global (GUID) do item do pedido.
    /// </summary>
    public Guid Id { get; set; } // Identificador unico do item no pedido

    /// <summary>
    /// Identificador do produto que foi adicionado ao pedido.
    /// </summary>
    public Guid ProductId { get; set; } // ID do produto comprado

    /// <summary>
    /// Nome do produto no momento da compra.
    /// Armazenado para preservar o historico, mesmo que o produto seja renomeado depois.
    /// </summary>
    public string ProductName { get; set; } = string.Empty; // Nome do produto conforme constava no momento do pedido

    /// <summary>
    /// Quantidade de unidades do produto solicitadas neste item.
    /// </summary>
    public int Quantity { get; set; } // Quantidade comprada deste produto

    /// <summary>
    /// Preco unitario do produto aplicado neste item (ja considerando precos personalizados, se houver).
    /// </summary>
    public decimal UnitPrice { get; set; } // Preco por unidade no momento da compra

    /// <summary>
    /// Preco total do item (UnitPrice * Quantity).
    /// </summary>
    public decimal TotalPrice { get; set; } // Subtotal do item (preco unitario * quantidade)
}
