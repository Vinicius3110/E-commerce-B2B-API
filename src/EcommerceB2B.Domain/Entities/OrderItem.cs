// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// A classe OrderItem representa um item individual dentro de um pedido
// Cada item referencia um produto, uma quantidade e os preços unitário e total
// O preço total é calculado automaticamente no momento da criação
public class OrderItem
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    private OrderItem()
    {
    }

    // Construtor público: cria um item de pedido vinculando produto, quantidade e preço
    // O preço total é calculado automaticamente (quantidade * preço unitário)
    public OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        // Valida que o ID do produto não é vazio
        if (productId == Guid.Empty)
        {
            // Lança exceção informando que o produto é obrigatório
            throw new DomainException("O ID do produto é obrigatório.");
        }

        // Valida que a quantidade é pelo menos 1 (não faz sentido pedir 0 itens)
        if (quantity < 1)
        {
            // Lança exceção informando que a quantidade mínima é 1
            throw new DomainException("A quantidade deve ser pelo menos 1.");
        }

        // Valida que o preço unitário é maior que zero
        // Preço zero significaria produto gratuito, o que não é esperado em B2B
        if (unitPrice <= 0)
        {
            // Lança exceção informando que o preço deve ser positivo
            throw new DomainException("O preço unitário deve ser maior que zero.");
        }

        // Gera um identificador único para o item do pedido
        Id = Guid.NewGuid();

        // Atribui os valores validados às propriedades
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;

        // Calcula o preço total multiplicando quantidade pelo preço unitário
        // O cálculo é feito no construtor para garantir consistência imediata
        TotalPrice = quantity * unitPrice;
    }

    // Identificador único do item do pedido
    public Guid Id { get; private set; }

    // Chave estrangeira: ID do pedido ao qual este item pertence
    // Será definido quando o item for adicionado a um pedido
    public Guid OrderId { get; private set; }

    // Chave estrangeira: ID do produto sendo comprado neste item
    public Guid ProductId { get; private set; }

    // Quantidade de unidades do produto neste item
    // Deve ser pelo menos 1 (validado no construtor)
    public int Quantity { get; private set; }

    // Preço unitário do produto no momento da compra
    // Congela o preço para evitar que alterações futuras no produto afetem pedidos existentes
    public decimal UnitPrice { get; private set; }

    // Preço total calculado: quantidade * preço unitário
    // Armazenado para consulta rápida sem precisar recalcular
    public decimal TotalPrice { get; private set; }

    // Propriedade de navegação: pedido ao qual este item pertence
    // virtual permite lazy loading pelo EF Core
    public virtual Order Order { get; private set; } = null!;

    // Propriedade de navegação: produto referenciado por este item
    public virtual Product Product { get; private set; } = null!;
}
