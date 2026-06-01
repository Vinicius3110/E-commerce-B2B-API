// Importa a entidade Company que este repositório gerencia
using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para a entidade Company
// A interface fica no Domain (camada interna), mas a implementação fica no Infrastructure
// Isso é inversão de dependência: o Domain define o contrato, o Infrastructure implementa
public interface ICompanyRepository
{
    // Busca uma empresa pelo seu ID único
    // Retorna null se não encontrar (por isso o tipo é Company?)
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Busca uma empresa pelo documento (CNPJ)
    // Útil para validar unicidade do CNPJ no cadastro
    Task<Company?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);

    // Adiciona uma nova empresa ao banco de dados
    Task AddAsync(Company company, CancellationToken cancellationToken = default);

    // Atualiza os dados de uma empresa existente
    // Não retorna nada — a entidade passada por referência já contém as alterações
    void Update(Company company);
}
