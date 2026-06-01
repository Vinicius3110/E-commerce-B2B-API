// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Empresa
namespace EcommerceB2B.Application.DTOs.Company;

/// <summary>
/// DTO utilizado para criar um novo usuario vinculado a uma empresa.
/// Usado por administradores da empresa para cadastrar funcionarios no sistema.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// Nome completo do usuario que sera cadastrado.
    /// Campo obrigatorio com limite de 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório.")] // Nome do usuario e obrigatorio para identificacao
    [MaxLength(100)] // Limita o nome a 100 caracteres para padronizacao
    public string Name { get; set; } = string.Empty; // Nome completo do novo usuario

    /// <summary>
    /// Endereco de e-mail do usuario, utilizado para login e notificacoes.
    /// Deve ser unico no sistema e estar em formato valido.
    /// </summary>
    [Required(ErrorMessage = "O e-mail é obrigatório.")] // E-mail obrigatorio para autenticacao e comunicacao
    [EmailAddress] // Valida automaticamente o formato do e-mail informado
    public string Email { get; set; } = string.Empty; // E-mail unico do usuario no sistema

    /// <summary>
    /// Senha inicial do usuario.
    /// Deve ter no minimo 8 caracteres; o usuario podera altera-la posteriormente.
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória.")] // Senha obrigatoria para seguranca da conta
    [MinLength(8)] // Comprimento minimo de 8 caracteres para seguranca basica
    public string Password { get; set; } = string.Empty; // Senha de acesso do usuario

    /// <summary>
    /// Perfil (role) do usuario dentro da empresa.
    /// Exemplos: "Admin", "Manager", "SalesRepresentative", "Viewer".
    /// </summary>
    [Required(ErrorMessage = "O perfil (role) é obrigatório.")] // Role obrigatoria para definir as permissoes do usuario
    public string Role { get; set; } = string.Empty; // Perfil de acesso do usuario na empresa
}
