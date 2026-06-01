// Importa a entidade de domínio que este repositório gerencia
using EcommerceB2B.Domain.Entities;

// Importa a interface (contrato) que este repositório implementa
// A interface está na camada Domain, a implementação na Infrastructure
// Isso é inversão de dependência: o Domain define o contrato, o Infrastructure implementa
using EcommerceB2B.Domain.Interfaces;

// Importa o AppDbContext que está na camada de persistência
using EcommerceB2B.Infrastructure.Persistence;

// Importa o Entity Framework Core para acesso ao banco de dados
using Microsoft.EntityFrameworkCore;

// Namespace que organiza as implementações de repositórios
// Cada repositório é responsável pela persistência de uma única entidade (SRP)
namespace EcommerceB2B.Infrastructure.Repositories;

// Implementação concreta do repositório de empresas
// Herda da interface ICompanyRepository definida na camada de Domain
// Esta classe é a ponte entre o domínio e o banco de dados PostgreSQL
public class CompanyRepository : ICompanyRepository
{
    // Contexto do banco de dados injetado via DI (Injeção de Dependência)
    // readonly garante que o contexto não será substituído após a construção
    // AppDbContext contém todos os DbSets e configurações de mapeamento
    private readonly AppDbContext _context;

    // Construtor que recebe o AppDbContext por injeção de dependência
    // O contêiner DI do ASP.NET Core resolve e injeta o contexto automaticamente
    public CompanyRepository(AppDbContext context)
    {
        // Armazena a referência do contexto para uso nos métodos do repositório
        _context = context;
    }

    // Busca uma empresa pelo seu ID único (chave primária)
    // Retorna null se a empresa não for encontrada (Company?)
    // cancellationToken permite cancelar a operação assíncrona se o cliente desistir
    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FindAsync busca primeiro no cache de entidades rastreadas pelo EF Core
        // Se não estiver no cache, faz a consulta no banco de dados
        // Passamos o id dentro de um object array porque FindAsync aceita params object[]
        return await _context.Companies.FindAsync(new object[] { id }, cancellationToken);
    }

    // Busca uma empresa pelo documento (CNPJ)
    // Útil para validação de unicidade durante o cadastro de novas empresas
    // O banco tem índice único em Document, então a busca é rápida
    public async Task<Company?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        // FirstOrDefaultAsync busca o primeiro registro que satisfaz a condição
        // Retorna null se nenhum registro for encontrado
        // A comparação é case-sensitive por padrão (depende da collation do banco)
        return await _context.Companies.FirstOrDefaultAsync(
            c => c.Document == document, // Condição lambda: documento igual ao informado
            cancellationToken);           // Passa o token de cancelamento
    }

    // Adiciona uma nova empresa ao banco de dados
    // A entidade company já foi validada pelo construtor rico da classe Company
    // O ID já foi gerado (Guid.NewGuid()) e os campos obrigatórios já foram validados
    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        // AddAsync adiciona a entidade ao rastreador de mudanças (Change Tracker)
        // O estado da entidade é marcado como Added
        // A inserção real no banco acontece quando SaveChangesAsync é chamado (via Unit of Work)
        await _context.Companies.AddAsync(company, cancellationToken);
    }

    // Atualiza os dados de uma empresa existente no banco de dados
    // A entidade já contém as alterações feitas pelos métodos de domínio (ex: SetName)
    // Não é async porque apenas marca a entidade como modificada (não acessa o banco)
    public void Update(Company company)
    {
        // Update marca a entidade como Modified no Change Tracker
        // Quando SaveChangesAsync for chamado, o EF Core gerará um UPDATE
        // com apenas os campos que realmente mudaram (change tracking automático)
        _context.Companies.Update(company);
    }
}
