// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Pedido
namespace EcommerceB2B.Application.DTOs.Order;

/// <summary>
/// DTO utilizado para atualizar o status de um pedido.
/// Usado pelo fornecedor para avancar o pedido no fluxo (ex: Confirmar, Enviar, Concluir).
/// </summary>
public class UpdateOrderStatusDto
{
    /// <summary>
    /// Novo status a ser atribuido ao pedido.
    /// Valores validos: "Confirmed", "Shipped", "Delivered", "Cancelled".
    /// A transicao de status e validada pela regra de negocio (ex: nao pode pular etapas).
    /// </summary>
    [Required(ErrorMessage = "O status é obrigatório.")] // Status e obrigatorio para atualizar o pedido
    public string Status { get; set; } = string.Empty; // Novo status do pedido
}
