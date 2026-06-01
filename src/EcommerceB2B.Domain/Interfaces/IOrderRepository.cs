using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Order
public interface IOrderRepository
{
    // Busca um pedido pelo ID, incluindo os itens e as empresas envolvidas
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Lista pedidos de uma empresa (como compradora ou vendedora)
    // O parâmetro asBuyer controla se busca pedidos onde a empresa é compradora ou vendedora
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        bool asBuyer,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    // Adiciona um novo pedido
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    // Atualiza um pedido existente (usado para mudanças de status)
    void Update(Order order);
}
