// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado para efetivamente redefinir a senha do usuario.
/// O usuario recebeu o UserId e o Token no e-mail apos solicitar "esqueci minha senha".
/// </summary>
public class ResetPasswordRequestDto
{
    /// <summary>
    /// Identificador unico do usuario que esta redefinindo sua senha.
    /// Foi enviado por e-mail junto com o token de redefinicao.
    /// </summary>
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")] // Necessario para localizar o usuario no banco de dados
    public string UserId { get; set; } = string.Empty; // Identificador do usuario (GUID como string)

    /// <summary>
    /// Token de redefinicao de senha gerado pelo ASP.NET Core Identity.
    /// Garante que a solicitacao de redefinicao e legitima e veio do e-mail do usuario.
    /// </summary>
    [Required(ErrorMessage = "O token é obrigatório.")] // O token valida que o usuario realmente recebeu o e-mail de redefinicao
    public string Token { get; set; } = string.Empty; // Token criptografico gerado pelo Identity para redefinir senha

    /// <summary>
    /// Nova senha que substituira a senha atual do usuario.
    /// A senha deve ter no minimo 8 caracteres para atender requisitos minimos de seguranca.
    /// </summary>
    [Required(ErrorMessage = "A nova senha é obrigatória.")] // A nova senha e obrigatoria para concluir a redefinicao
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")] // Define o comprimento minimo da nova senha
    public string NewPassword { get; set; } = string.Empty; // Nova senha que sera atribuida a conta do usuario
}
