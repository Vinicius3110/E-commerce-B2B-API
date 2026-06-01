// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado para receber as credenciais de login do usuario.
/// Contem apenas os campos essenciais para autenticacao: e-mail e senha.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Endereco de e-mail cadastrado do usuario.
    /// Deve ser um e-mail valido e e um campo obrigatorio.
    /// </summary>
    [Required(ErrorMessage = "O e-mail é obrigatório.")] // Garante que o campo de e-mail seja preenchido
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")] // Valida se o texto esta no formato correto de endereco de e-mail
    public string Email { get; set; } = string.Empty; // Inicializa com string vazia para evitar null reference

    /// <summary>
    /// Senha do usuario para autenticacao.
    /// Campo obrigatorio, sem validacao de complexidade aqui (a complexidade e definida no Identity).
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória.")] // A senha e essencial para o processo de login, portanto obrigatoria
    public string Password { get; set; } = string.Empty; // Inicializa com string vazia para evitar valores nulos
}
