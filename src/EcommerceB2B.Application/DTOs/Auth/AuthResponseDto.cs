// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO de resposta enviado ao cliente apos uma autenticacao bem-sucedida (login ou refresh token).
/// Contem os tokens JWT necessarios para acesso aos recursos protegidos da API.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Token JWT de acesso (Access Token) que deve ser enviado no header Authorization.
    /// Possui curta duracao (ex: 15 a 30 minutos) e contem as claims do usuario.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty; // Token JWT utilizado para autorizacao nas requisicoes HTTP

    /// <summary>
    /// Token de atualizacao (Refresh Token) usado para obter um novo Access Token sem necessidade de novo login.
    /// Possui longa duracao (ex: 7 a 30 dias) e deve ser armazenado de forma segura.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty; // Token para renovar o Access Token sem reautenticacao do usuario

    /// <summary>
    /// Data e hora UTC em que o Access Token ira expirar.
    /// O cliente deve usar esta informacao para solicitar um novo token antes da expiracao.
    /// </summary>
    public DateTime ExpiresAt { get; set; } // Momento exato da expiracao do token atual (UTC)

    /// <summary>
    /// Tipo do token de acesso, sempre "Bearer" conforme o padrao OAuth 2.0 / JWT.
    /// O cliente deve prefixar o Access Token com este valor ao montar o header Authorization.
    /// </summary>
    public string TokenType { get; set; } = "Bearer"; // Tipo de token conforme especificacao OAuth 2.0 (Bearer)
}
