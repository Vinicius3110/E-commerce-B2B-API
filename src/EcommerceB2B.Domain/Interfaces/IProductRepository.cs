using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Product
public interface IProductRepository
{
    // Busca um produto pelo ID, incluindo a empresa vendedora e a categoria
    // O parâmetro includeDetails controla se as propriedades de navegação são carregadas
    Task<Product?> GetByIdAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default);

    // Lista produtos com filtros e paginação
    // Cada parâmetro opcional refina a busca; se nulo, o filtro não é aplicado
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? sellerCompanyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    // Verifica se um SKU já existe para uma empresa vendedora (regra de unicidade)
    Task<bool> SkuExistsAsync(Guid companyId, string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    // Adiciona um novo produto
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    // Atualiza um produto existente
    void Update(Product product);

    // Adiciona um preço customizado para uma empresa compradora
    Task AddPriceAsync(ProductPrice price, CancellationToken cancellationToken = default);
}
