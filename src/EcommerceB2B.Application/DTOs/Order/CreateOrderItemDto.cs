// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Pedido
namespace EcommerceB2B.Application.DTOs.Order;

/// <summary>
/// DTO que representa um item a ser adicionado ao criar um novo pedido.
/// Contem a referencia ao produto e a quantidade desejada.
/// </summary>
public class CreateOrderItemDto
{
    /// <summary>
    /// Identificador do produto que se deseja comprar.
    /// Deve referenciar um produto existente e ativo no catalogo do fornecedor.
    /// </summary>
    [Required(ErrorMessage = "O produto é obrigatório.")] // Produto obrigatorio para o item do pedido
    public Guid ProductId { get; set; } // ID do produto a ser comprado (GUID)

    /// <summary>
    /// Quantidade desejada do produto.
    /// Deve ser no minimo 1; quantidades maiores sao validadas contra o estoque disponivel.
    /// </summary>
    [Required(ErrorMessage = "A quantidade é obrigatória.")] // Quantidade obrigatoria
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")] // Deve ser >= 1
    public int Quantity { get; set; } // Quantidade de unidades a comprar
}
