using EcommerceB2B.Domain.Entities;

namespace EcommerceB2B.Domain.Interfaces;

// Interface que define o contrato de persistência para Category
public interface ICategoryRepository
{
    // Busca uma categoria pelo ID
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Lista todas as categorias ativas (para exibição no catálogo)
    // IReadOnlyList garante que o resultado não será modificado pelo consumidor
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);

    // Adiciona uma nova categoria
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    // Atualiza uma categoria existente
    void Update(Category category);
}
