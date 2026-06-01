// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Empresa
namespace EcommerceB2B.Application.DTOs.Company;

/// <summary>
/// DTO utilizado para atualizar os dados de uma empresa existente.
/// Contem validacoes para garantir que os dados obrigatorios sejam fornecidos.
/// </summary>
public class UpdateCompanyDto
{
    /// <summary>
    /// Novo nome da empresa (razao social ou nome fantasia atualizado).
    /// Campo obrigatorio com limite de 200 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome da empresa é obrigatório.")] // Nome da empresa e obrigatorio para atualizacao
    [MaxLength(200)] // Limita o nome a 200 caracteres para consistencia com o banco de dados
    public string Name { get; set; } = string.Empty; // Novo nome da empresa

    /// <summary>
    /// Tipo da empresa: "Supplier" (fornecedora) ou "Buyer" (compradora).
    /// O tipo determina as permissoes e funcionalidades disponiveis para a empresa.
    /// </summary>
    [Required(ErrorMessage = "O tipo da empresa é obrigatório.")] // O tipo e obrigatorio pois define permissoes e funcionalidades
    public string Type { get; set; } = string.Empty; // Tipo da empresa: Supplier ou Buyer
}
