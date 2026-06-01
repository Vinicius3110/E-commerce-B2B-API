// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado para requisitar a renovacao do Access Token atraves do Refresh Token.
/// O cliente envia apenas o Refresh Token obtido no login ou na ultima renovacao.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Refresh Token que foi emitido durante o login ou ultima renovacao.
    /// A API validara se o token ainda e valido e nao foi revogado.
    /// </summary>
    [Required(ErrorMessage = "O refresh token é obrigatório.")] // Campo obrigatorio; sem ele nao e possivel renovar o Access Token
    public string RefreshToken { get; set; } = string.Empty; // Inicializa com string vazia por seguranca contra null reference
}
