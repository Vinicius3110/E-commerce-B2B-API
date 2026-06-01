// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Categoria
namespace EcommerceB2B.Application.DTOs.Category;

/// <summary>
/// DTO utilizado para criar uma nova categoria de produtos.
/// Contem validacoes para garantir dados obrigatorios e limites de tamanho.
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// Nome da categoria a ser criada.
    /// Campo obrigatorio com limite de 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")] // Nome e obrigatorio para identificar a categoria
    [MaxLength(100)] // Limita o nome a 100 caracteres para consistencia de interface
    public string Name { get; set; } = string.Empty; // Nome da nova categoria

    /// <summary>
    /// Descricao opcional da categoria com informacoes adicionais.
    /// Pode ter ate 500 caracteres; util para explicar o escopo da categoria.
    /// </summary>
    [MaxLength(500)] // Limita a descricao a 500 caracteres para evitar textos excessivamente longos
    public string? Description { get; set; } // Descricao opcional da categoria (pode ser nula)
}
