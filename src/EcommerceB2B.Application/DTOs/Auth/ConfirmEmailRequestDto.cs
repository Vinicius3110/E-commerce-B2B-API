// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado para confirmar o endereco de e-mail do usuario apos o registro.
/// O usuario recebe um link com o userId e o token de confirmacao por e-mail.
/// </summary>
public class ConfirmEmailRequestDto
{
    /// <summary>
    /// Identificador unico do usuario cujo e-mail esta sendo confirmado.
    /// Este ID foi enviado no link de confirmacao encaminhado por e-mail.
    /// </summary>
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")] // O ID do usuario e necessario para localizar o registro correto no banco
    public string UserId { get; set; } = string.Empty; // Identificador do usuario no sistema (GUID como string)

    /// <summary>
    /// Token de confirmacao gerado pelo ASP.NET Core Identity.
    /// E um codigo criptografico que valida a posse do e-mail pelo usuario.
    /// </summary>
    [Required(ErrorMessage = "O token é obrigatório.")] // O token e obrigatorio para validar a confirmacao de e-mail
    public string Token { get; set; } = string.Empty; // Token gerado pelo Identity para verificacao do e-mail
}
