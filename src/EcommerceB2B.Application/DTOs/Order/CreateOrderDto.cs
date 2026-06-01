// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Pedido
namespace EcommerceB2B.Application.DTOs.Order;

/// <summary>
/// DTO utilizado para criar um novo pedido de compra.
/// O comprador especifica o fornecedor e a lista de itens que deseja adquirir.
/// </summary>
public class CreateOrderDto
{
    /// <summary>
    /// Identificador da empresa vendedora/fornecedora (Supplier) para a qual o pedido sera enviado.
    /// Obrigatorio pois um pedido deve estar associado a um fornecedor especifico.
    /// </summary>
    [Required(ErrorMessage = "O fornecedor é obrigatório.")] // Fornecedor obrigatorio para processar o pedido
    public Guid SellerCompanyId { get; set; } // ID da empresa fornecedora (GUID)

    /// <summary>
    /// Lista de itens que compoe o pedido.
    /// Deve conter no minimo 1 item; nao faz sentido criar pedido sem produtos.
    /// </summary>
    [Required(ErrorMessage = "O pedido deve conter pelo menos um item.")] // Pelo menos um item e obrigatorio
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")] // Garante minimo de 1 item na lista
    public List<CreateOrderItemDto> Items { get; set; } = new(); // Lista de itens do pedido (inicializada vazia)
}
