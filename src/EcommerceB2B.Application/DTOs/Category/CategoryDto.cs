// Define o namespace do DTO dentro da camada de Aplicacao, na area de Categoria
namespace EcommerceB2B.Application.DTOs.Category;

/// <summary>
/// DTO que representa os dados de uma categoria de produtos para leitura (retorno em consultas).
/// Categorias organizam os produtos em grupos logicos, facilitando a navegacao.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Identificador unico global (GUID) da categoria no sistema.
    /// </summary>
    public Guid Id { get; set; } // Identificador unico da categoria (UUID)

    /// <summary>
    /// Nome da categoria (ex: "Eletronicos", "Vestuario", "Alimentos").
    /// Deve ser unico e descritivo para facilitar a busca.
    /// </summary>
    public string Name { get; set; } = string.Empty; // Nome descritivo da categoria

    /// <summary>
    /// Descricao opcional da categoria com detalhes sobre os tipos de produtos que ela agrupa.
    /// Pode ser nulo se a categoria nao tiver descricao cadastrada.
    /// </summary>
    public string? Description { get; set; } // Descricao opcional da categoria (pode ser nula)

    /// <summary>
    /// Indica se a categoria esta ativa e visivel na plataforma.
    /// Categorias inativas e seus produtos nao aparecem nas buscas.
    /// </summary>
    public bool IsActive { get; set; } // Status de atividade da categoria (true = ativa)
}
