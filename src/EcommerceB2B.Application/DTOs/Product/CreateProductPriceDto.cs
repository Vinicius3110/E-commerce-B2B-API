// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Produto
namespace EcommerceB2B.Application.DTOs.Product;

/// <summary>
/// DTO utilizado para criar um preco personalizado de produto para um comprador especifico.
/// No modelo B2B, fornecedores podem oferecer precos diferentes por cliente e por quantidade minima.
/// </summary>
public class CreateProductPriceDto
{
    /// <summary>
    /// Identificador da empresa compradora (Buyer) para a qual o preco personalizado se aplica.
    /// </summary>
    [Required(ErrorMessage = "A empresa compradora é obrigatória.")] // Comprador obrigatorio para precificacao personalizada
    public Guid BuyerCompanyId { get; set; } // ID da empresa compradora (GUID)

    /// <summary>
    /// Preco unitario personalizado oferecido ao comprador especifico.
    /// Deve ser maior que zero (minimo 0.01).
    /// </summary>
    [Required(ErrorMessage = "O preço personalizado é obrigatório.")] // Preco obrigatorio
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço personalizado deve ser maior que zero.")] // Preco deve ser positivo
    public decimal CustomPrice { get; set; } // Preco especial para o comprador

    /// <summary>
    /// Quantidade minima de itens que o comprador deve adquirir para obter este preco personalizado.
    /// Deve ser no minimo 1 (nao faz sentido preco especial para zero unidades).
    /// </summary>
    [Required(ErrorMessage = "A quantidade mínima é obrigatória.")] // Quantidade minima obrigatoria
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade mínima deve ser pelo menos 1.")] // Deve ser >= 1
    public int MinQuantity { get; set; } // Quantidade minima para aplicar o preco personalizado
}
