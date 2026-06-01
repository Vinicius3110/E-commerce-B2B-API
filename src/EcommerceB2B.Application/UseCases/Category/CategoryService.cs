// Importa os DTOs de categoria definidos na camada de aplicacao
using EcommerceB2B.Application.DTOs.Category;

// Importa a entidade Category do dominio
using EcommerceB2B.Domain.Entities;

// Alias para evitar conflito entre o nome da entidade Category e o namespace Category
// O compilador confundiria Category (entidade) com Category (namespace deste arquivo)
using CategoryEntity = EcommerceB2B.Domain.Entities.Category;

// Importa a excecao customizada de dominio para erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Importa a interface de repositorio de categorias definida no dominio
using EcommerceB2B.Domain.Interfaces;

// Namespace que agrupa os servicos de categoria na camada de aplicacao
namespace EcommerceB2B.Application.UseCases.Category;

// Servico de categoria: orquestra operacoes de CRUD relacionadas a categorias de produtos
// Categorias organizam produtos em grupos logicos (ex: Eletronicos, Vestuario, Alimentos)
// Depende exclusivamente do repositorio de categorias
public class CategoryService
{
    // Repositorio de categorias (definido no dominio, implementado no Infrastructure)
    // Fornece acesso a dados: busca, listagem, criacao e atualizacao
    private readonly ICategoryRepository _categoryRepository;

    // Construtor que recebe o repositorio por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao concreta
    public CategoryService(ICategoryRepository categoryRepository)
    {
        // Armazena a referencia do repositorio
        _categoryRepository = categoryRepository;
    }

    // Caso de uso: Listar todas as categorias ativas
    // Busca apenas categorias com IsActive = true para exibicao no catalogo
    // Retorna uma lista de CategoryDto para consumo pela API
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Busca todas as categorias ativas no repositorio
        // GetActiveAsync retorna apenas categorias onde IsActive = true
        var categories = await _categoryRepository.GetActiveAsync(cancellationToken);

        // Converte cada entidade Category para CategoryDto usando LINQ Select
        // ToList materializa a consulta e executa o mapeamento
        return categories.Select(MapToDto).ToList();
    }

    // Caso de uso: Criar uma nova categoria de produtos
    // Recebe os dados do DTO, cria a entidade e persiste no banco
    // Retorna o DTO da categoria criada com o ID gerado
    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        // Cria a entidade Category usando o construtor publico
        // O construtor valida o nome (nao pode ser vazio) e define IsActive = true
        // A descricao e opcional e pode ser nula
        var category = new CategoryEntity(request.Name, request.Description);

        // Persiste a categoria no banco via repositorio
        // AddAsync adiciona ao ChangeTracker do EF Core
        await _categoryRepository.AddAsync(category, cancellationToken);

        // Converte a entidade criada para DTO e retorna
        return MapToDto(category);
    }

    // Caso de uso: Atualizar uma categoria existente
    // Busca, valida existencia, aplica alteracoes e persiste
    // Retorna o DTO da categoria atualizada
    public async Task<CategoryDto> UpdateAsync(
        Guid id,
        UpdateCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        // Busca a categoria pelo ID no repositorio
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        // Se a categoria nao for encontrada, lanca excecao de dominio
        if (category is null)
        {
            throw new DomainException("Categoria não encontrada.");
        }

        // Atualiza o nome da categoria usando o metodo SetName da entidade
        // SetName valida se o nome nao e vazio ou apenas espacos
        category.SetName(request.Name);

        // Atualiza a descricao da categoria usando o metodo SetDescription
        // A descricao e opcional — pode ser nula para remover a descricao existente
        category.SetDescription(request.Description);

        // Persiste as alteracoes no banco via repositorio
        // Update marca a entidade como modificada no ChangeTracker do EF Core
        _categoryRepository.Update(category);

        // Converte a entidade atualizada para DTO e retorna
        return MapToDto(category);
    }

    // Metodo auxiliar privado: converte entidade Category para CategoryDto
    // Centraliza o mapeamento para evitar duplicacao nos metodos publicos
    // E static pois nao depende de estado da instancia
    private static CategoryDto MapToDto(CategoryEntity category)
    {
        // Cria e retorna o DTO preenchido com os dados da entidade
        return new CategoryDto
        {
            // Copia o identificador unico da categoria
            Id = category.Id,

            // Copia o nome da categoria (ex: "Eletronicos")
            Name = category.Name,

            // Copia a descricao (pode ser nula)
            Description = category.Description,

            // Copia o status de atividade
            IsActive = category.IsActive
        };
    }
}
