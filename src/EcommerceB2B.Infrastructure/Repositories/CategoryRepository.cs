// Importa a entidade de domínio que este repositório gerencia
using EcommerceB2B.Domain.Entities;

// Importa a interface do repositório definida na camada de Domain
using EcommerceB2B.Domain.Interfaces;

// Importa o AppDbContext que está na camada de persistência
using EcommerceB2B.Infrastructure.Persistence;

// Importa o Entity Framework Core para operações de banco de dados
using Microsoft.EntityFrameworkCore;

// Namespace que organiza as implementações de repositórios na camada de Infrastructure
namespace EcommerceB2B.Infrastructure.Repositories;

// Implementação concreta do repositório de categorias
// Implementa ICategoryRepository, seguindo o princípio de inversão de dependência
// A camada Domain define o contrato, a Infrastructure fornece a implementação
public class CategoryRepository : ICategoryRepository
{
    // Contexto do Entity Framework Core injetado via DI
    // Fornece acesso a todas as tabelas do banco de dados
    // readonly impede que a referência seja alterada acidentalmente
    private readonly AppDbContext _context;

    // Construtor que recebe o AppDbContext por injeção de dependência
    // O ASP.NET Core gerencia o ciclo de vida do contexto (Scoped por requisição HTTP)
    public CategoryRepository(AppDbContext context)
    {
        // Armazena a referência do contexto para uso nos métodos de acesso a dados
        _context = context;
    }

    // Busca uma categoria pelo seu ID único
    // Retorna null se a categoria não existir no banco de dados
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FindAsync é otimizado: busca primeiro no cache local do EF Core
        // Se a entidade já estiver sendo rastreada, retorna a instância em memória
        // Caso contrário, executa SELECT no banco de dados
        return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
    }

    // Retorna todas as categorias ativas ordenadas por nome
    // Usado para popular listas de seleção no frontend (dropdowns, filtros)
    // IReadOnlyList garante que o resultado é imutável pelo consumidor
    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        // Where(c => c.IsActive) filtra apenas categorias ativas (soft delete)
        // OrderBy(c => c.Name) ordena alfabeticamente pelo nome
        // AsNoTracking() desabilita o change tracking para melhorar performance
        // Como é apenas leitura, não precisamos rastrear as entidades
        // ToListAsync() executa a query e materializa os resultados
        return await _context.Categories
            .Where(c => c.IsActive)      // Filtra apenas categorias ativas (IsActive == true)
            .OrderBy(c => c.Name)        // Ordena por nome em ordem alfabética crescente
            .AsNoTracking()              // Desabilita change tracking (somente leitura, mais rápido)
            .ToListAsync(cancellationToken); // Executa a query e retorna a lista materializada
    }

    // Adiciona uma nova categoria ao banco de dados
    // A entidade já passou por validação de domínio (construtor rico)
    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        // AddAsync registra a nova categoria no Change Tracker com estado Added
        // A inserção SQL será gerada quando SaveChangesAsync for chamado
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    // Atualiza uma categoria existente no banco de dados
    // As alterações foram feitas pelos métodos de domínio (ex: SetName, SetDescription)
    public void Update(Category category)
    {
        // Update marca a entidade inteira como Modified no Change Tracker
        // O EF Core otimiza gerando UPDATE apenas para os campos que realmente mudaram
        _context.Categories.Update(category);
    }
}
