// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Produto
namespace EcommerceB2B.Application.DTOs.Product;

/// <summary>
/// DTO utilizado para atualizar os dados de um produto existente.
/// Semelhante ao CreateProductDto, porem sem o campo CategoryId (categoria nao e alterada diretamente aqui).
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Nome atualizado do produto.
    /// Campo obrigatorio com limite de 200 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome do produto é obrigatório.")] // Nome obrigatorio
    [MaxLength(200)] // Limita a 200 caracteres
    public string Name { get; set; } = string.Empty; // Nome atualizado do produto

    /// <summary>
    /// Descricao atualizada do produto.
    /// Campo opcional com limite de 2000 caracteres.
    /// </summary>
    [MaxLength(2000)] // Limita a 2000 caracteres
    public string? Description { get; set; } // Descricao opcional atualizada

    /// <summary>
    /// SKU atualizado do produto.
    /// Campo obrigatorio com limite de 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O SKU é obrigatório.")] // SKU obrigatorio
    [MaxLength(50)] // Limita a 50 caracteres
    public string Sku { get; set; } = string.Empty; // SKU atualizado

    /// <summary>
    /// Preco base atualizado do produto.
    /// Deve ser maior que zero (minimo 0.01).
    /// </summary>
    [Required(ErrorMessage = "O preço base é obrigatório.")] // Preco obrigatorio
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço base deve ser maior que zero.")] // Preco positivo
    public decimal BasePrice { get; set; } // Preco base atualizado

    /// <summary>
    /// Quantidade em estoque atualizada.
    /// Deve ser zero ou maior (nao pode ser negativa).
    /// </summary>
    [Required(ErrorMessage = "A quantidade em estoque é obrigatória.")] // Estoque obrigatorio
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque não pode ser negativa.")] // Estoque >= 0
    public int StockQuantity { get; set; } // Estoque atualizado
}
