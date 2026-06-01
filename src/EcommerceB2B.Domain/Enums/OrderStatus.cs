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
