// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Categoria
namespace EcommerceB2B.Application.DTOs.Category;

/// <summary>
/// DTO utilizado para atualizar uma categoria de produtos existente.
/// Mesma estrutura do CreateCategoryDto, garantindo consistencia nas operacoes.
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Nome atualizado da categoria.
    /// Campo obrigatorio com limite de 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")] // Nome obrigatorio para atualizacao
    [MaxLength(100)] // Limita o nome a 100 caracteres
    public string Name { get; set; } = string.Empty; // Nome atualizado da categoria

    /// <summary>
    /// Descricao atualizada da categoria.
    /// Campo opcional com limite de 500 caracteres.
    /// </summary>
    [MaxLength(500)] // Limita a descricao a 500 caracteres para manter padronizacao
    public string? Description { get; set; } // Descricao opcional atualizada (pode ser nula)
}
