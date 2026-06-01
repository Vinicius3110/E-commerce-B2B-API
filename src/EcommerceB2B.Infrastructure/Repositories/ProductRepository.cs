// Importa as entidades de domínio gerenciadas por este repositório
using EcommerceB2B.Domain.Entities;

// Importa a interface do repositório definida na camada de Domain
using EcommerceB2B.Domain.Interfaces;

// Importa o AppDbContext que está na camada de persistência
using EcommerceB2B.Infrastructure.Persistence;

// Importa o Entity Framework Core para consultas e operações no banco de dados
using Microsoft.EntityFrameworkCore;

// Namespace que organiza as implementações de repositórios
namespace EcommerceB2B.Infrastructure.Repositories;

// Implementação concreta do repositório de produtos
// Gerencia a persistência de Product e ProductPrice (preços customizados)
// O repositório é a única porta de acesso a dados para a entidade Product
public class ProductRepository : IProductRepository
{
    // Contexto do banco de dados injetado via DI
    private readonly AppDbContext _context;

    // Construtor com injeção do AppDbContext
    // O ciclo de vida Scoped garante que o mesmo contexto é usado em toda a requisição
    public ProductRepository(AppDbContext context)
    {
        // Armazena o contexto para uso nos métodos de acesso a dados
        _context = context;
    }

    // Busca um produto pelo ID, com opção de incluir dados relacionados
    // includeDetails = true carrega Company, Category e CustomPrices (eager loading)
    // includeDetails = false carrega apenas o produto (mais rápido)
    public async Task<Product?> GetByIdAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        // Inicia a query a partir do DbSet Products
        var query = _context.Products.AsQueryable();

        // Se solicitado, inclui os dados relacionados (eager loading)
        // Include() carrega as propriedades de navegação na mesma consulta SQL (JOIN)
        if (includeDetails)
        {
            // Include(p => p.Company) faz JOIN com a tabela Companies
            // Include(p => p.Category) faz JOIN com a tabela Categories
            // ThenInclude(p => p.CustomPrices) faz JOIN com ProductPrices
            // Isso evita o problema N+1: carrega tudo em uma única query
            query = query
                .Include(p => p.Company)     // Carrega a empresa vendedora
                .Include(p => p.Category)    // Carrega a categoria do produto
                .Include(p => p.CustomPrices); // Carrega os preços customizados
        }

        // FirstOrDefaultAsync busca o produto pelo ID com os includes definidos
        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    // Lista produtos com filtros combináveis e paginação
    // Cada parâmetro é opcional: se null, o filtro não é aplicado
    // Retorna uma tupla com a lista de itens e o total de registros (para paginação no frontend)
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? sellerCompanyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Inicia a query base com todos os produtos ativos
        // Apenas produtos ativos aparecem no catálogo público
        var query = _context.Products
            .Where(p => p.IsActive)      // Filtro base: apenas produtos ativos
            .AsQueryable();

        // Aplica filtro de categoria (se informado)
        // Cada filtro é aplicado condicionalmente usando if
        if (categoryId.HasValue)
        {
            // Filtra produtos que pertencem à categoria especificada
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Aplica filtro de empresa vendedora (se informado)
        if (sellerCompanyId.HasValue)
        {
            // Filtra produtos que pertencem à empresa vendedora especificada
            query = query.Where(p => p.CompanyId == sellerCompanyId.Value);
        }

        // Aplica filtro de preço mínimo (se informado)
        if (minPrice.HasValue)
        {
            // Filtra produtos com BasePrice >= preço mínimo
            query = query.Where(p => p.BasePrice >= minPrice.Value);
        }

        // Aplica filtro de preço máximo (se informado)
        if (maxPrice.HasValue)
        {
            // Filtra produtos com BasePrice <= preço máximo
            query = query.Where(p => p.BasePrice <= maxPrice.Value);
        }

        // Conta o total de registros que satisfazem os filtros
        // CountAsync executa um SELECT COUNT(*) no banco
        // É importante contar ANTES da paginação para obter o total real
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplica paginação: Skip() pula os registros das páginas anteriores
        // Take() limita o resultado ao tamanho da página
        // Inclui dados relacionados para exibição completa no catálogo
        var items = await query
            .Include(p => p.Company)     // Carrega dados da empresa vendedora (nome, etc.)
            .Include(p => p.Category)    // Carrega dados da categoria (nome, etc.)
            .OrderBy(p => p.Name)        // Ordena por nome do produto (consistente entre páginas)
            .Skip((page - 1) * pageSize) // Pula os registros das páginas anteriores
            .Take(pageSize)              // Limita ao número de itens por página
            .AsNoTracking()              // Desabilita tracking (somente leitura, melhor performance)
            .ToListAsync(cancellationToken); // Executa a query e materializa os resultados

        // Retorna a tupla com itens paginados e contagem total
        return (items, totalCount);
    }

    // Verifica se um SKU já existe para uma empresa vendedora
    // Implementa a regra de unicidade: SKU único por empresa
    // excludeProductId permite excluir o produto atual em operações de update
    // (ao editar um produto, seu próprio SKU não deve ser considerado duplicado)
    public async Task<bool> SkuExistsAsync(Guid companyId, string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        // Inicia a query verificando empresa e SKU
        var query = _context.Products.Where(p =>
            p.CompanyId == companyId &&     // Mesma empresa
            p.Sku == sku);                  // Mesmo SKU

        // Se um productId foi informado para exclusão (cenário de update)
        // Exclui o próprio produto da verificação de duplicidade
        if (excludeProductId.HasValue)
        {
            // Exclui o produto com o ID especificado da busca de duplicados
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        // AnyAsync retorna true se existir pelo menos um registro que satisfaz a condição
        // Mais eficiente que CountAsync > 0 pois para na primeira ocorrência
        return await query.AnyAsync(cancellationToken);
    }

    // Adiciona um novo produto ao banco de dados
    // A entidade já foi validada pelo construtor rico do domínio
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        // AddAsync registra o novo produto no Change Tracker com estado Added
        await _context.Products.AddAsync(product, cancellationToken);
    }

    // Atualiza os dados de um produto existente
    // As alterações foram feitas pelos métodos de domínio do Product
    public void Update(Product product)
    {
        // Update marca a entidade como Modified no Change Tracker
        // O EF Core gerará um UPDATE otimizado apenas com os campos alterados
        _context.Products.Update(product);
    }

    // Adiciona um preço customizado para um produto/empresa compradora
    // ProductPrice é uma entidade separada com sua própria tabela
    public async Task AddPriceAsync(ProductPrice price, CancellationToken cancellationToken = default)
    {
        // AddAsync registra o novo preço customizado no Change Tracker
        // A validação de unicidade (ProductId + CompanyId) é feita em nível de banco
        await _context.ProductPrices.AddAsync(price, cancellationToken);
    }
}
