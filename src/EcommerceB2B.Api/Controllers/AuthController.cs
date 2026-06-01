// Importa a interface do servico de autenticacao (registro, login, tokens, etc.)
using EcommerceB2B.Application.Interfaces;

// Importa os DTOs de autenticacao que sao usados como entrada e saida dos endpoints
using EcommerceB2B.Application.DTOs.Auth;

// Importa o ASP.NET Core MVC para atributos como [ApiController], [Route], [HttpPost]
using Microsoft.AspNetCore.Mvc;

// Namespace que organiza os controllers da API REST
namespace EcommerceB2B.Api.Controllers;

// Controller responsavel pelos endpoints de autenticacao (registro, login, tokens, senha)
// [ApiController] ativa comportamentos automaticos:
//   - Validacao automatica de ModelState (retorna 400 se dados invalidos)
//   - Binding automatico de parametros do corpo da requisicao
//   - Respostas padronizadas com ProblemDetails para erros
// [Route("api/auth")] define o prefixo de rota: todas as actions respondem em /api/auth/*
// Nao possui [Authorize] pois e um controller PUBLICO (endpoints de login/registro)
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Servico de autenticacao injetado via DI (implementa IAuthService)
    // Contem toda a logica de negocios de autenticacao:
    //   - Registro de empresa + administrador
    //   - Login com JWT
    //   - Renovacao de token (refresh)
    //   - Confirmacao de e-mail
    //   - Recuperacao e redefinicao de senha
    //   - Logout (revogacao de refresh token)
    private readonly IAuthService _authService;

    // Construtor que recebe o IAuthService por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao concreta (AuthService)
    public AuthController(IAuthService authService)
    {
        // Armazena a referencia do servico de autenticacao
        _authService = authService;
    }

    // POST api/auth/register
    // Endpoint publico para registrar uma nova empresa e seu administrador
    // Fluxo:
    //   1. Recebe dados da empresa e do admin no corpo da requisicao (JSON)
    //   2. Cria a empresa (tenant), o usuario admin e envia e-mail de confirmacao
    //   3. Retorna tokens JWT (access + refresh) para acesso imediato
    // [FromBody] vincula os dados do corpo JSON ao DTO RegisterRequestDto
    // CancellationToken permite cancelar a operacao se o cliente desconectar
    // Retorna:
    //   201 Created com AuthResponseDto (access token, refresh token, expiracao)
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)] // Documenta resposta de sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de validacao/negocio
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico de autenticacao para processar o registro
        // RegisterAsync cria empresa + usuario admin + envia e-mail de confirmacao
        var result = await _authService.RegisterAsync(request, cancellationToken);

        // CreatedAtAction retorna 201 Created com a localizacao do recurso criado
        // Neste caso, nao temos um endpoint GET para o recurso, usamos o nome vazio
        // O corpo da resposta contem os tokens JWT
        return Created(string.Empty, result);
    }

    // POST api/auth/confirm-email
    // Endpoint publico para confirmar o endereco de e-mail do usuario
    // O usuario clicou no link de confirmacao recebido por e-mail apos o registro
    // O frontend extrai userId e token da URL e os envia neste endpoint
    // Fluxo:
    //   1. Valida o token de confirmacao de e-mail
    //   2. Se valido, marca o e-mail como confirmado (EmailConfirmed = true)
    //   3. Apos confirmacao, o usuario pode fazer login normalmente
    // Retorna:
    //   200 OK em caso de sucesso (sem corpo, apenas status)
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Documenta confirmacao bem-sucedida
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta token invalido/expirado
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico para processar a confirmacao do e-mail
        // ConfirmEmailAsync valida o token e marca o email como confirmado
        // Se o token for invalido, lanca DomainException (capturada pelo middleware)
        await _authService.ConfirmEmailAsync(request, cancellationToken);

        // Ok() retorna 200 OK sem corpo — a confirmacao foi bem-sucedida
        return Ok();
    }

    // POST api/auth/login
    // Endpoint publico para autenticar um usuario existente
    // Fluxo:
    //   1. Recebe email e senha no corpo da requisicao
    //   2. Verifica credenciais (email + senha)
    //   3. Verifica se o email foi confirmado (RequireConfirmedEmail = true)
    //   4. Gera tokens JWT (access + refresh) e retorna
    // Se as credenciais forem invalidas, lanca DomainException (capturada pelo middleware)
    // Retorna:
    //   200 OK com AuthResponseDto (access token, refresh token, expiracao)
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)] // Documenta login bem-sucedido
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta credenciais invalidas
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico de autenticacao para validar credenciais e gerar tokens
        // LoginAsync retorna AuthResponseDto com access e refresh tokens
        var result = await _authService.LoginAsync(request, cancellationToken);

        // Ok() retorna 200 OK com o corpo contendo os tokens JWT
        return Ok(result);
    }

    // POST api/auth/refresh
    // Endpoint publico para renovar o access token usando o refresh token
    // Fluxo:
    //   1. Recebe o refresh token no corpo da requisicao
    //   2. Valida se o refresh token e valido e nao foi revogado
    //   3. Gera um novo par de tokens (access + refresh)
    //   4. Revoga o refresh token antigo (refresh token rotation)
    // Se o refresh token for invalido/expirado, lanca DomainException
    // Retorna:
    //   200 OK com novo AuthResponseDto (novos tokens)
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)] // Documenta renovacao bem-sucedida
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta refresh token invalido
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico para renovar o access token via refresh token
        // RefreshTokenAsync implementa refresh token rotation para seguranca
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);

        // Ok() retorna 200 OK com os novos tokens
        return Ok(result);
    }

    // POST api/auth/forgot-password
    // Endpoint publico para iniciar recuperacao de senha (esqueci minha senha)
    // Fluxo:
    //   1. Recebe o email do usuario
    //   2. Gera um token de redefinicao de senha
    //   3. Envia o token por e-mail para o endereco informado
    // Por seguranca, a resposta e sempre a mesma independente do email existir ou nao
    // Isso impede que atacantes descubram quais emails estao cadastrados
    // Retorna:
    //   200 OK sempre (nao revela se o email existe)
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Documenta resposta padrao
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico para iniciar o processo de recuperacao de senha
        // ForgotPasswordAsync gera token e envia email sem revelar se o usuario existe
        await _authService.ForgotPasswordAsync(request, cancellationToken);

        // Ok() retorna 200 OK — mensagem padrao independente do resultado
        return Ok();
    }

    // POST api/auth/reset-password
    // Endpoint publico para redefinir a senha usando o token recebido por e-mail
    // Fluxo:
    //   1. Recebe userId, token de redefinicao e nova senha
    //   2. Valida o token de redefinicao
    //   3. Se valido, substitui a senha antiga pela nova senha
    // Se o token for invalido ou expirado, lanca DomainException
    // Retorna:
    //   200 OK em caso de redefinicao bem-sucedida
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Documenta redefinicao bem-sucedida
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta token invalido
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico para processar a redefinicao de senha
        // ResetPasswordAsync valida o token e define a nova senha
        await _authService.ResetPasswordAsync(request, cancellationToken);

        // Ok() retorna 200 OK — senha redefinida com sucesso
        return Ok();
    }

    // POST api/auth/logout
    // Endpoint publico para invalidar o refresh token (logout)
    // Fluxo:
    //   1. Recebe o refresh token no corpo da requisicao
    //   2. Revoga (invalida) o refresh token
    //   3. Apos o logout, o refresh token nao pode mais renovar o access token
    // O access token continua valido ate expirar (nao e possivel revoga-lo)
    // O frontend deve descartar ambos os tokens apos o logout
    // Retorna:
    //   200 OK — logout bem-sucedido
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Documenta logout bem-sucedido
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequestDto request)
    {
        // Chama o servico para revogar o refresh token
        // LogoutAsync e idempotente: se o token ja estiver revogado, nao faz nada
        // Enviamos o RefreshToken do RefreshTokenRequestDto diretamente
        await _authService.LogoutAsync(request.RefreshToken);

        // Ok() retorna 200 OK — logout concluido
        return Ok();
    }
}
