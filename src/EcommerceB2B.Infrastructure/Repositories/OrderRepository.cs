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

// Implementação concreta do repositório de pedidos
// Gerencia a persistência de Order e seus OrderItems
// Um pedido é a entidade central do fluxo B2B: envolve comprador, vendedor e itens
public class OrderRepository : IOrderRepository
{
    // Contexto do banco de dados injetado via DI
    private readonly AppDbContext _context;

    // Construtor com injeção do AppDbContext
    // O contexto é compartilhado entre todos os repositórios na mesma requisição (Scoped)
    public OrderRepository(AppDbContext context)
    {
        // Armazena o contexto para uso nos métodos de acesso a dados
        _context = context;
    }

    // Busca um pedido completo pelo ID, incluindo:
    // - Itens do pedido (OrderItems) e seus produtos (Product)
    // - Empresa compradora (BuyerCompany)
    // - Empresa vendedora (SellerCompany)
    // Isso evita o problema N+1: carrega tudo em uma única consulta SQL com JOINs
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Constrói a query com todos os includes necessários
        // Include() carrega propriedades de navegação de primeiro nível
        // ThenInclude() carrega propriedades de navegação de segundo nível
        return await _context.Orders
            .Include(o => o.Items)           // Carrega os itens do pedido (OrderItems)
                .ThenInclude(oi => oi.Product) // Dentro de cada item, carrega o produto referenciado
            .Include(o => o.BuyerCompany)    // Carrega a empresa compradora
            .Include(o => o.SellerCompany)   // Carrega a empresa vendedora
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken); // Busca pelo ID
    }

    // Lista pedidos de uma empresa, como compradora OU como vendedora
    // O parâmetro asBuyer controla qual papel da empresa será consultado
    // Retorna tupla com lista paginada e contagem total para paginação no frontend
    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        bool asBuyer,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Inicia a query base a partir do DbSet Orders
        var query = _context.Orders.AsQueryable();

        // Filtra por comprador OU vendedor baseado no parâmetro asBuyer
        if (asBuyer)
        {
            // Busca pedidos onde a empresa é a compradora
            // Inclui a empresa vendedora para mostrar de quem está comprando
            query = query
                .Where(o => o.BuyerCompanyId == companyId) // Filtra por comprador
                .Include(o => o.SellerCompany);             // Inclui dados do vendedor
        }
        else
        {
            // Busca pedidos onde a empresa é a vendedora
            // Inclui a empresa compradora para mostrar quem está comprando
            query = query
                .Where(o => o.SellerCompanyId == companyId) // Filtra por vendedor
                .Include(o => o.BuyerCompany);              // Inclui dados do comprador
        }

        // Conta o total de pedidos que satisfazem o filtro (antes da paginação)
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplica paginação e ordenação
        // Os pedidos mais recentes aparecem primeiro (ordem decrescente de criação)
        var items = await query
            .OrderByDescending(o => o.CreatedAt) // Ordena do mais recente para o mais antigo
            .Skip((page - 1) * pageSize)          // Pula registros das páginas anteriores
            .Take(pageSize)                       // Limita ao tamanho da página
            .AsNoTracking()                       // Desabilita tracking (leitura, mais rápido)
            .ToListAsync(cancellationToken);      // Executa a query e materializa os resultados

        // Retorna a tupla com itens paginados e contagem total
        return (items, totalCount);
    }

    // Adiciona um novo pedido ao banco de dados
    // O pedido já foi validado pelo construtor rico do domínio
    // Os itens do pedido são salvos automaticamente por cascade
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        // AddAsync registra o novo pedido no Change Tracker com estado Added
        // Os OrderItems associados também serão rastreados e salvos em cascade
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    // Atualiza os dados de um pedido existente
    // Usado principalmente para mudanças de status (Confirm, Cancel, Ship, Deliver)
    // O UpdatedAt é atualizado automaticamente pelos métodos de domínio
    public void Update(Order order)
    {
        // Update marca a entidade como Modified no Change Tracker
        // O EF Core gerará um UPDATE apenas com os campos que realmente mudaram
        _context.Orders.Update(order);
    }
}
