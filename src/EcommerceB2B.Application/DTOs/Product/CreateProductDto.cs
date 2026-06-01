// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Produto
namespace EcommerceB2B.Application.DTOs.Product;

/// <summary>
/// DTO utilizado para criar um novo produto no catalogo da empresa fornecedora.
/// Contem validacoes rigorosas para garantir dados consistentes no cadastro.
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Identificador da categoria a qual o produto pertence.
    /// Campo obrigatorio; o produto deve sempre estar vinculado a uma categoria existente.
    /// </summary>
    [Required(ErrorMessage = "A categoria é obrigatória.")] // Categoria obrigatoria para organizacao do catalogo
    public Guid CategoryId { get; set; } // ID da categoria do produto (GUID)

    /// <summary>
    /// Nome do produto.
    /// Campo obrigatorio com limite de 200 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome do produto é obrigatório.")] // Nome obrigatorio para identificacao do produto
    [MaxLength(200)] // Limita o nome a 200 caracteres
    public string Name { get; set; } = string.Empty; // Nome comercial do produto

    /// <summary>
    /// Descricao detalhada do produto (opcional).
    /// Pode conter ate 2000 caracteres com especificacoes, materiais, dimensoes, etc.
    /// </summary>
    [MaxLength(2000)] // Limite de 2000 caracteres para a descricao detalhada
    public string? Description { get; set; } // Descricao opcional do produto

    /// <summary>
    /// Codigo SKU (Stock Keeping Unit) do produto.
    /// Deve ser unico por fornecedor para controle de estoque.
    /// </summary>
    [Required(ErrorMessage = "O SKU é obrigatório.")] // SKU obrigatorio para rastreamento de inventario
    [MaxLength(50)] // Limita o SKU a 50 caracteres
    public string Sku { get; set; } = string.Empty; // Codigo SKU unico do produto

    /// <summary>
    /// Preco base do produto em valor monetario.
    /// Deve ser maior que zero (valor minimo de 0.01 para evitar precos zerados).
    /// </summary>
    [Required(ErrorMessage = "O preço base é obrigatório.")] // Preco obrigatorio para comercializacao
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço base deve ser maior que zero.")] // Preco deve ser positivo e maior que zero
    public decimal BasePrice { get; set; } // Preco base do produto (decimal)

    /// <summary>
    /// Quantidade inicial em estoque do produto.
    /// Deve ser zero ou um valor positivo (nao pode ser negativo).
    /// </summary>
    [Required(ErrorMessage = "A quantidade em estoque é obrigatória.")] // Estoque obrigatorio para controle
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque não pode ser negativa.")] // Estoque deve ser >= 0
    public int StockQuantity { get; set; } // Quantidade inicial em estoque
}
