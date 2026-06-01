// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// Importa os tipos enumerados do domínio (OrderStatus) para gerenciar o ciclo de vida do pedido
using EcommerceB2B.Domain.Enums;

// A classe Order representa um pedido de compra no sistema B2B
// Gerencia o ciclo de vida completo: criação, confirmação, envio e entrega
// Contém validações de transição de status que garantem o fluxo correto do negócio
public class Order
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    private Order()
    {
    }

    // Construtor público: cria um novo pedido entre duas empresas com itens obrigatórios
    // Valida que comprador e vendedor são diferentes e que há pelo menos um item
    public Order(Guid buyerCompanyId, Guid sellerCompanyId, IEnumerable<OrderItem> items)
    {
        // Valida que o ID da empresa compradora não é vazio
        if (buyerCompanyId == Guid.Empty)
        {
            // Lança exceção informando que o comprador é obrigatório
            throw new DomainException("O ID da empresa compradora é obrigatório.");
        }

        // Valida que o ID da empresa vendedora não é vazio
        if (sellerCompanyId == Guid.Empty)
        {
            // Lança exceção informando que o vendedor é obrigatório
            throw new DomainException("O ID da empresa vendedora é obrigatório.");
        }

        // Valida que comprador e vendedor são empresas diferentes
        // Regra de negócio: uma empresa não pode negociar consigo mesma
        if (buyerCompanyId == sellerCompanyId)
        {
            // Lança exceção informando que auto-compra não é permitida
            throw new DomainException("Uma empresa não pode fazer um pedido para si mesma.");
        }

        // Valida que a lista de itens não é nula
        if (items is null)
        {
            // Lança exceção informando que os itens são obrigatórios
            throw new DomainException("O pedido deve conter pelo menos um item.");
        }

        // Converte para lista para evitar múltiplas enumerações e verificar contagem
        var itemList = items.ToList();

        // Valida que há pelo menos um item no pedido
        if (itemList.Count == 0)
        {
            // Lança exceção informando que é necessário pelo menos um item
            throw new DomainException("O pedido deve conter pelo menos um item.");
        }

        // Gera um identificador único para o pedido
        Id = Guid.NewGuid();

        // Atribui os identificadores das empresas
        BuyerCompanyId = buyerCompanyId;
        SellerCompanyId = sellerCompanyId;

        // Todo pedido novo começa com status Pendente (aguardando confirmação do vendedor)
        Status = OrderStatus.Pendente;

        // Inicializa a coleção de itens e adiciona os itens fornecidos
        Items = new List<OrderItem>(itemList);

        // Calcula o valor total do pedido somando os totais de cada item
        RecalculateTotal();

        // Registra a data/hora de criação em UTC
        CreatedAt = DateTime.UtcNow;

        // Inicializa UpdatedAt com a mesma data de criação
        UpdatedAt = DateTime.UtcNow;
    }

    // Identificador único do pedido
    public Guid Id { get; private set; }

    // Chave estrangeira: ID da empresa compradora (quem fez o pedido)
    public Guid BuyerCompanyId { get; private set; }

    // Chave estrangeira: ID da empresa vendedora (quem recebe o pedido)
    public Guid SellerCompanyId { get; private set; }

    // Status atual do pedido no fluxo de negócio
    // Controla em qual etapa do ciclo de vida o pedido se encontra
    public OrderStatus Status { get; private set; }

    // Valor total do pedido (soma dos preços totais de todos os itens)
    // Calculado e atualizado sempre que os itens são modificados
    public decimal TotalAmount { get; private set; }

    // Data e hora de criação do pedido (UTC)
    public DateTime CreatedAt { get; private set; }

    // Data e hora da última atualização do pedido (UTC)
    // Atualizado a cada mudança de status ou alteração nos itens
    public DateTime UpdatedAt { get; private set; }

    // Propriedade de navegação: coleção de itens do pedido
    // virtual permite lazy loading pelo EF Core
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    // Propriedade de navegação: empresa compradora
    public virtual Company BuyerCompany { get; private set; } = null!;

    // Propriedade de navegação: empresa vendedora
    public virtual Company SellerCompany { get; private set; } = null!;

    // Método privado para recalcular o valor total do pedido
    // Percorre todos os itens e soma seus preços totais
    // É privado pois o recálculo é um detalhe interno da entidade
    private void RecalculateTotal()
    {
        // Soma o TotalPrice de cada item da coleção
        // LINQ Sum com lambda é eficiente e legível
        TotalAmount = Items.Sum(i => i.TotalPrice);
    }

    // Método auxiliar para marcar o timestamp de atualização
    // Chamado sempre que o pedido sofre uma alteração de estado
    private void MarkAsUpdated()
    {
        // Atualiza a data/hora da última modificação para o momento atual (UTC)
        UpdatedAt = DateTime.UtcNow;
    }

    // Confirma o pedido: transição de Pendente para Confirmado
    // Apenas o vendedor pode confirmar um pedido que está pendente
    public void Confirm()
    {
        // Valida que o status atual é Pendente (único status que permite confirmação)
        if (Status != OrderStatus.Pendente)
        {
            // Lança exceção informando que apenas pedidos pendentes podem ser confirmados
            throw new DomainException("Apenas pedidos com status Pendente podem ser confirmados.");
        }

        // Altera o status para Confirmado
        Status = OrderStatus.Confirmado;

        // Registra o momento da confirmação
        MarkAsUpdated();
    }

    // Cancela o pedido: transição de Pendente ou Confirmado para Cancelado
    // Pode ser cancelado pelo comprador (antes da confirmação) ou por acordo entre as partes
    public void Cancel()
    {
        // Valida que o status atual permite cancelamento
        // Apenas pedidos Pendente ou Confirmado podem ser cancelados
        if (Status != OrderStatus.Pendente && Status != OrderStatus.Confirmado)
        {
            // Lança exceção informando os status que permitem cancelamento
            throw new DomainException("Apenas pedidos com status Pendente ou Confirmado podem ser cancelados.");
        }

        // Altera o status para Cancelado
        Status = OrderStatus.Cancelado;

        // Registra o momento do cancelamento
        MarkAsUpdated();
    }

    // Despacha o pedido: transição de Confirmado para Enviado
    // Apenas o vendedor pode despachar, e somente após confirmar o pedido
    public void Ship()
    {
        // Valida que o status atual é Confirmado (único status que permite envio)
        if (Status != OrderStatus.Confirmado)
        {
            // Lança exceção informando que apenas pedidos confirmados podem ser enviados
            throw new DomainException("Apenas pedidos com status Confirmado podem ser enviados.");
        }

        // Altera o status para Enviado
        Status = OrderStatus.Enviado;

        // Registra o momento do envio
        MarkAsUpdated();
    }

    // Entrega o pedido: transição de Enviado para Entregue
    // Status final do ciclo de vida do pedido
    public void Deliver()
    {
        // Valida que o status atual é Enviado (único status que permite entrega)
        if (Status != OrderStatus.Enviado)
        {
            // Lança exceção informando que apenas pedidos enviados podem ser entregues
            throw new DomainException("Apenas pedidos com status Enviado podem ser entregues.");
        }

        // Altera o status para Entregue (status final)
        Status = OrderStatus.Entregue;

        // Registra o momento da entrega
        MarkAsUpdated();
    }
}
