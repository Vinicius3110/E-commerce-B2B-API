// Importa os DTOs de autenticacao que serao usados como entrada e saida dos metodos
using EcommerceB2B.Application.DTOs.Auth;

// Namespace que agrupa as interfaces da camada de aplicacao
// As interfaces definem contratos que serao consumidos pelos Controllers da API
// e implementados pelas classes de servico em UseCases
namespace EcommerceB2B.Application.Interfaces;

// Interface que define as operacoes de autenticacao disponiveis no sistema
// A interface fica na Application e e consumida pelos Controllers da API
// A implementacao concreta (AuthService) fica em UseCases e depende do Infrastructure
// Segue o principio de segregacao de interface (ISP): apenas metodos de autenticacao
public interface IAuthService
{
    // Registra uma nova empresa e seu administrador no sistema
    // Cria a empresa, o usuario admin e envia e-mail de confirmacao de cadastro
    // Retorna os tokens JWT (access + refresh) para o novo usuario
    // O CancellationToken permite que a operacao seja cancelada se a requisicao HTTP for abortada
    Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    // Autentica um usuario existente usando email e senha
    // Verifica se o email foi confirmado e se a conta nao esta bloqueada
    // Retorna os tokens JWT (access + refresh) em caso de sucesso
    // Em caso de falha, lanca DomainException com a mensagem de erro apropriada
    Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    // Renova o access token expirado usando um refresh token valido
    // O refresh token e validado, um novo par de tokens e gerado e o antigo e revogado
    // Isso implementa o padrao de "refresh token rotation" para maior seguranca
    // Se o refresh token for invalido ou expirado, lanca DomainException
    Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    // Confirma o endereco de e-mail do usuario usando o token enviado por e-mail
    // O usuario clicou no link de confirmacao recebido apos o registro
    // Apos a confirmacao, o usuario pode fazer login normalmente
    // Se o token for invalido ou o usuario nao existir, lanca DomainException
    Task ConfirmEmailAsync(
        ConfirmEmailRequestDto request,
        CancellationToken cancellationToken = default);

    // Inicia o processo de recuperacao de senha (esqueci minha senha)
    // Gera um token de redefinicao e envia por e-mail para o usuario
    // Por seguranca, nao revela se o email existe ou nao no sistema
    // A resposta e sempre a mesma independente do email existir
    Task ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default);

    // Redefine a senha do usuario usando o token recebido por e-mail
    // O token e validado e a nova senha substitui a senha anterior
    // Se o token for invalido ou o usuario nao existir, lanca DomainException
    Task ResetPasswordAsync(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default);

    // Invalida o refresh token do usuario (logout)
    // Apos o logout, o refresh token nao pode mais ser usado para renovar o access token
    // O usuario precisara fazer login novamente para obter novos tokens
    // Se o token ja estiver revogado, a operacao nao faz nada (idempotente)
    Task LogoutAsync(string refreshToken);
}
