// Importa os DTOs de autenticacao definidos na camada de aplicacao
using EcommerceB2B.Application.DTOs.Auth;

// Importa a interface IAuthService que esta classe implementa
using EcommerceB2B.Application.Interfaces;

// Importa as entidades do dominio (Company, CompanyUser)
using EcommerceB2B.Domain.Entities;

// Alias para evitar conflito entre o nome da entidade Company e o sibling namespace Company
// Estando em EcommerceB2B.Application.UseCases.Auth, o compilador tambem enxerga
// EcommerceB2B.Application.UseCases.Company como namespace, causando ambiguidade
using CompanyEntity = EcommerceB2B.Domain.Entities.Company;

// Importa os tipos enumerados do dominio (CompanyType)
using EcommerceB2B.Domain.Enums;

// Importa a excecao customizada de dominio para erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Importa as interfaces de repositorio definidas na camada de dominio
using EcommerceB2B.Domain.Interfaces;

// Importa os servicos de autenticacao da camada de infraestrutura
// JwtService: gera e valida tokens JWT
// RefreshTokenService: gerencia refresh tokens em memoria
// EmailService: envia emails simulados de confirmacao e redefinicao de senha
using EcommerceB2B.Infrastructure.Auth;

// Importa o ASP.NET Core Identity para gerenciamento de usuarios
using Microsoft.AspNetCore.Identity;

// Importa a interface de logging para registrar eventos e erros
using Microsoft.Extensions.Logging;

// Namespace que agrupa os servicos de autenticacao na camada de aplicacao
namespace EcommerceB2B.Application.UseCases.Auth;

// Servico de autenticacao: implementa IAuthService com todas as operacoes de auth
// Orquestra UserManager, SignInManager, repositorios e servicos de infraestrutura
// Cada metodo implementa um caso de uso completo do fluxo de autenticacao
public class AuthService : IAuthService
{
    // Gerenciador de usuarios do ASP.NET Core Identity
    // Responsavel por criar, buscar, atualizar e deletar usuarios
    // Tambem gerencia roles, claims e tokens de confirmacao/redefinicao
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    // Gerenciador de login (SignIn) do ASP.NET Core Identity
    // Responsavel por verificar senhas e gerenciar lockout (bloqueio por tentativas)
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;

    // Repositorio de empresas (definido no dominio, implementado no Infrastructure)
    // Usado para verificar unicidade de CNPJ e criar novas empresas
    private readonly ICompanyRepository _companyRepository;

    // Servico de geracao e validacao de tokens JWT (Infrastructure)
    // Gera access tokens com claims do usuario para autorizacao nas requisicoes
    private readonly JwtService _jwtService;

    // Servico de gerenciamento de refresh tokens (Infrastructure)
    // Armazena, valida e revoga refresh tokens em memoria (ConcurrentDictionary)
    private readonly RefreshTokenService _refreshTokenService;

    // Servico de envio de emails (Infrastructure) – implementacao simulada
    // Envia emails de confirmacao de cadastro e redefinicao de senha
    private readonly EmailService _emailService;

    // Logger tipado para a classe AuthService
    // Usado para registrar eventos importantes (registros, logins, erros)
    private readonly ILogger<AuthService> _logger;

    // Construtor que recebe todas as dependencias por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta cada parametro automaticamente
    // Todas as dependencias sao armazenadas como readonly para imutabilidade
    public AuthService(
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager,
        ICompanyRepository companyRepository,
        JwtService jwtService,
        RefreshTokenService refreshTokenService,
        EmailService emailService,
        ILogger<AuthService> logger)
    {
        // Armazena cada dependencia no campo correspondente
        _userManager = userManager;
        _signInManager = signInManager;
        _companyRepository = companyRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _emailService = emailService;
        _logger = logger;
    }

    // Caso de uso: Registro de nova empresa com administrador
    // Fluxo completo:
    //   1. Validar unicidade do CNPJ
    //   2. Criar empresa no banco
    //   3. Criar usuario Identity (admin)
    //   4. Atribuir role "Admin"
    //   5. Gerar e enviar token de confirmacao de email
    //   6. Gerar tokens JWT e retornar resposta
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra no log o inicio do processo de registro
        // LogInformation registra no nivel Information (informativo, nao erro)
        _logger.LogInformation(
            "Iniciando registro de empresa: {CompanyName} com CNPJ: {Document}",
            request.CompanyName,
            request.Document);

        // 1. Verifica se ja existe uma empresa com o mesmo CNPJ
        // GetByDocumentAsync busca no banco pelo documento informado
        var existingCompany = await _companyRepository.GetByDocumentAsync(
            request.Document,
            cancellationToken);

        // Se ja existir empresa com este CNPJ, lanca excecao de dominio
        if (existingCompany is not null)
        {
            // Lanca DomainException com mensagem clara em portugues
            throw new DomainException("Já existe uma empresa cadastrada com este CNPJ.");
        }

        // 2. Cria a entidade Company com os dados do DTO
        // O construtor da Company valida nome e documento automaticamente
        var company = new CompanyEntity(request.CompanyName, request.Document);

        // Persiste a empresa no banco de dados via repositorio
        // AddAsync adiciona ao ChangeTracker do EF Core (o SaveChanges ocorre depois)
        await _companyRepository.AddAsync(company, cancellationToken);

        // 3. Cria o usuario Identity para o administrador da empresa
        // IdentityUser<Guid> e a classe base do Identity com chave Guid
        var user = new IdentityUser<Guid>
        {
            // UserName e obrigatorio no Identity — usamos o email como nome de usuario
            // Isso simplifica o login: usuario digita email e senha
            UserName = request.Email,

            // Email para contato e recuperacao de senha
            Email = request.Email,

            // EmailConfirmed = false: usuario precisa confirmar o email antes de fazer login
            // Isso garante que o email informado realmente pertence ao usuario
            EmailConfirmed = false
        };

        // Tenta criar o usuario no Identity com a senha fornecida
        // CreateAsync valida a senha conforme as regras configuradas (digitos, maiusculas, etc.)
        var createResult = await _userManager.CreateAsync(user, request.Password);

        // Se a criacao falhar (senha fraca, email duplicado, etc.), lanca excecao
        if (!createResult.Succeeded)
        {
            // Concatena todos os erros do Identity em uma unica mensagem
            // Select extrai a propriedade Description de cada IdentityError
            // string.Join une todas as descricoes separadas por ponto e virgula
            var errors = string.Join("; ",
                createResult.Errors.Select(e => e.Description));

            // Lanca DomainException com todos os erros de validacao do Identity
            throw new DomainException($"Falha ao criar usuário: {errors}");
        }

        // 4. Atribui a role "Admin" ao usuario
        // AddToRoleAsync adiciona o usuario ao perfil de Administrador
        // Isso permite que o middleware de autorizacao verifique [Authorize(Roles = "Admin")]
        var roleResult = await _userManager.AddToRoleAsync(user, "Admin");

        // Se a atribuicao de role falhar, lanca excecao
        if (!roleResult.Succeeded)
        {
            // Concatena os erros e lanca DomainException
            var errors = string.Join("; ",
                roleResult.Errors.Select(e => e.Description));
            throw new DomainException($"Falha ao atribuir perfil: {errors}");
        }

        // 5. Gera token de confirmacao de email via Identity
        // Este token e criptograficamente seguro e de uso unico
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Constroi o link de confirmacao que sera enviado por email
        // Uri.EscapeDataString codifica caracteres especiais no token para uso em URL
        // O link inclui userId e token como query parameters
        var confirmationLink = $"https://localhost:5001/api/auth/confirm-email" +
            $"?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

        // Envia o email de confirmacao (implementacao simulada via log)
        // Em producao, isso enviaria um email real para o usuario
        await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

        // Registra no log que o registro foi concluido com sucesso
        _logger.LogInformation(
            "Empresa {CompanyId} e usuario {UserId} criados com sucesso",
            company.Id,
            user.Id);

        // 6. Gera os tokens JWT para o novo usuario
        // O usuario ja pode usar a API mesmo antes de confirmar o email
        // Porem o endpoint de login exigira EmailConfirmed = true
        var accessToken = _jwtService.GenerateToken(
            user.Id,
            company.Id,
            "Admin",
            request.Email);

        // Gera o refresh token associado ao usuario e empresa
        var (refreshToken, refreshTokenExpiresAt) = _refreshTokenService.GenerateRefreshToken(
            user.Id,
            company.Id);

        // Retorna o DTO de resposta com os tokens e metadados
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer"
        };
    }

    // Caso de uso: Login (autenticacao) de usuario existente
    // Fluxo completo:
    //   1. Buscar usuario por email
    //   2. Verificar se email foi confirmado
    //   3. Verificar se conta nao esta bloqueada
    //   4. Verificar senha (com lockout em caso de falha)
    //   5. Obter roles do usuario
    //   6. Gerar tokens JWT e retornar resposta
    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra tentativa de login no log
        _logger.LogInformation("Tentativa de login para: {Email}", request.Email);

        // 1. Busca o usuario pelo email informado
        // FindByEmailAsync retorna null se o email nao estiver cadastrado
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Se o usuario nao for encontrado, lanca excecao
        if (user is null)
        {
            // Mensagem generica: nao revela se o email existe ou nao (seguranca)
            throw new DomainException("Email ou senha inválidos.");
        }

        // 2. Verifica se o email do usuario foi confirmado
        // EmailConfirmed e setado como true apos o usuario clicar no link de confirmacao
        if (!user.EmailConfirmed)
        {
            // Informa que o email precisa ser confirmado antes do login
            throw new DomainException(
                "Email não confirmado. Verifique sua caixa de entrada e confirme seu email antes de fazer login.");
        }

        // 3. Verifica se a conta esta bloqueada por excesso de tentativas
        // IsLockedOutAsync retorna true se a conta excedeu MaxFailedAccessAttempts
        if (await _userManager.IsLockedOutAsync(user))
        {
            // Informa que a conta esta temporariamente bloqueada
            throw new DomainException(
                "Conta bloqueada por excesso de tentativas. Tente novamente mais tarde.");
        }

        // 4. Verifica a senha usando o SignInManager
        // CheckPasswordSignInAsync verifica a senha sem criar cookie de autenticacao
        // lockoutOnFailure: true → incrementa o contador de tentativas falhas
        // Isso ativa o bloqueio automatico apos MaxFailedAccessAttempts tentativas
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        // Se a senha estiver incorreta ou conta bloqueada
        if (!signInResult.Succeeded)
        {
            // Se a conta foi bloqueada durante esta tentativa
            if (signInResult.IsLockedOut)
            {
                throw new DomainException(
                    "Conta bloqueada por excesso de tentativas. Tente novamente mais tarde.");
            }

            // Se o email nao foi confirmado (caso raro, ja verificado acima)
            if (signInResult.IsNotAllowed)
            {
                throw new DomainException(
                    "Email não confirmado. Verifique sua caixa de entrada.");
            }

            // Caso geral: senha incorreta
            throw new DomainException("Email ou senha inválidos.");
        }

        // 5. Obtem as roles (perfis) do usuario
        // GetRolesAsync retorna uma lista de strings com os nomes das roles
        var roles = await _userManager.GetRolesAsync(user);

        // Obtem a primeira role ou "Admin" como padrao
        // FirstOrDefault retorna null se a lista estiver vazia, entao usamos ?? "Admin"
        var role = roles.FirstOrDefault() ?? "Admin";

        // 6. Gera o access token JWT com as claims do usuario
        // companyId: Guid.Empty por enquanto — sera substituido quando CompanyUser estiver implementado
        // Em uma implementacao completa, buscariamos o CompanyId via CompanyUserRepository
        var accessToken = _jwtService.GenerateToken(
            user.Id,
            Guid.Empty,
            role,
            request.Email);

        // Gera o refresh token para o usuario
        var (refreshToken, refreshTokenExpiresAt) = _refreshTokenService.GenerateRefreshToken(
            user.Id,
            Guid.Empty);

        // Registra login bem-sucedido no log
        _logger.LogInformation(
            "Login bem-sucedido para: {Email} com role: {Role}",
            request.Email,
            role);

        // Retorna o DTO com os tokens gerados
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer"
        };
    }

    // Caso de uso: Renovacao de tokens usando refresh token
    // Fluxo completo:
    //   1. Validar o refresh token
    //   2. Buscar o usuario dono do token
    //   3. Obter roles do usuario
    //   4. Gerar novo access token e novo refresh token
    //   5. Revogar o refresh token antigo (rotacao de tokens)
    //   6. Retornar os novos tokens
    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra a solicitacao de refresh no log
        _logger.LogInformation("Solicitacao de refresh token recebida");

        // 1. Valida o refresh token no servico de infraestrutura
        // ValidateAndGetRefreshToken retorna null se o token nao existir ou estiver expirado
        var tokenEntry = _refreshTokenService.ValidateAndGetRefreshToken(request.RefreshToken);

        // Se o token for invalido, lanca excecao
        if (tokenEntry is null)
        {
            // O token pode estar expirado ou ja ter sido revogado
            throw new DomainException("Refresh token inválido ou expirado.");
        }

        // 2. Busca o usuario dono do refresh token
        // FindByIdAsync recebe o ID como string (Guid convertido para string)
        var user = await _userManager.FindByIdAsync(tokenEntry.UserId.ToString());

        // Se o usuario nao existir mais (foi deletado), lanca excecao
        if (user is null)
        {
            throw new DomainException("Usuário não encontrado.");
        }

        // 3. Obtem as roles atuais do usuario
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Admin";

        // 4. Gera um novo access token JWT
        // Usa o CompanyId armazenado no refresh token (contexto multi-tenant)
        var accessToken = _jwtService.GenerateToken(
            user.Id,
            tokenEntry.CompanyId,
            role,
            user.Email!);

        // Gera um novo refresh token (rotacao: novo token substitui o antigo)
        var (newRefreshToken, refreshTokenExpiresAt) = _refreshTokenService.GenerateRefreshToken(
            user.Id,
            tokenEntry.CompanyId);

        // 5. Revoga o refresh token antigo (impede reuso)
        // Esta e a estrategia de "refresh token rotation" que aumenta a seguranca
        // Se um token revogado for reusado, indica possivel roubo de token
        _refreshTokenService.RevokeToken(request.RefreshToken);

        // Registra a renovacao concluida no log
        _logger.LogInformation("Token renovado com sucesso para usuario: {UserId}", user.Id);

        // 6. Retorna o DTO com os novos tokens
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer"
        };
    }

    // Caso de uso: Confirmacao de email do usuario
    // Fluxo:
    //   1. Converter userId de string para Guid
    //   2. Buscar usuario pelo ID
    //   3. Confirmar email usando o token do Identity
    public async Task ConfirmEmailAsync(
        ConfirmEmailRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra a solicitacao de confirmacao no log
        _logger.LogInformation("Confirmacao de email para usuario: {UserId}", request.UserId);

        // 1. Converte o UserId de string para Guid
        // Guid.Parse lanca FormatException se a string nao for um Guid valido
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            // Se o formato for invalido, lanca excecao de dominio
            throw new DomainException("ID de usuário inválido.");
        }

        // 2. Busca o usuario pelo ID
        var user = await _userManager.FindByIdAsync(request.UserId);

        // Se o usuario nao existir, lanca excecao
        if (user is null)
        {
            throw new DomainException("Usuário não encontrado.");
        }

        // Se o email ja estiver confirmado, nao faz nada (operacao idempotente)
        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Email do usuario {UserId} ja estava confirmado", user.Id);
            return;
        }

        // 3. Confirma o email usando o token do Identity
        // ConfirmEmailAsync valida o token criptografico e seta EmailConfirmed = true
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        // Se a confirmacao falhar (token invalido/expirado), lanca excecao
        if (!result.Succeeded)
        {
            // Concatena os erros do Identity
            var errors = string.Join("; ",
                result.Errors.Select(e => e.Description));

            // Lanca excecao com os detalhes do erro
            throw new DomainException($"Falha ao confirmar email: {errors}");
        }

        // Registra confirmacao bem-sucedida no log
        _logger.LogInformation("Email confirmado com sucesso para usuario: {UserId}", user.Id);
    }

    // Caso de uso: Solicitar recuperacao de senha (esqueci minha senha)
    // Fluxo:
    //   1. Buscar usuario por email
    //   2. Se encontrado: gerar token de redefinicao
    //   3. Enviar email com link de redefinicao
    // IMPORTANTE: Por seguranca, nao revela se o email existe ou nao
    public async Task ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra a solicitacao de recuperacao no log
        _logger.LogInformation("Solicitacao de recuperacao de senha para: {Email}", request.Email);

        // 1. Busca o usuario pelo email informado
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Se o usuario nao existir, simplesmente nao faz nada
        // Esta e uma pratica de seguranca: nao revelamos se o email existe ou nao
        // Assim, um atacante nao consegue enumerar emails validos do sistema
        if (user is null)
        {
            // Registra que o email nao foi encontrado (apenas para debug interno)
            _logger.LogWarning(
                "Tentativa de recuperacao para email nao cadastrado: {Email}",
                request.Email);

            // Retorna sem erro — o usuario nao sabe se o email existe
            return;
        }

        // 2. Gera o token de redefinicao de senha via Identity
        // Este token e criptograficamente seguro e tem validade limitada
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Constroi o link de redefinicao que sera enviado por email
        // Inclui userId, token e email como query parameters
        // Uri.EscapeDataString codifica caracteres especiais para uso seguro em URL
        var resetLink = $"https://localhost:5001/api/auth/reset-password" +
            $"?userId={user.Id}" +
            $"&token={Uri.EscapeDataString(resetToken)}" +
            $"&email={Uri.EscapeDataString(request.Email)}";

        // 3. Envia o email de redefinicao de senha (implementacao simulada)
        await _emailService.SendPasswordResetAsync(request.Email, resetLink);

        // Registra que o email foi enviado no log
        _logger.LogInformation("Email de redefinicao enviado para: {Email}", request.Email);
    }

    // Caso de uso: Redefinir a senha usando token de recuperacao
    // Fluxo:
    //   1. Buscar usuario pelo ID
    //   2. Redefinir a senha usando o token e a nova senha
    //   3. O Identity valida o token e aplica a nova senha
    public async Task ResetPasswordAsync(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Registra a solicitacao de redefinicao no log
        _logger.LogInformation("Redefinicao de senha para usuario: {UserId}", request.UserId);

        // 1. Busca o usuario pelo ID informado
        var user = await _userManager.FindByIdAsync(request.UserId);

        // Se o usuario nao existir, lanca excecao
        if (user is null)
        {
            throw new DomainException("Usuário não encontrado.");
        }

        // 2. Redefine a senha usando o token e a nova senha
        // ResetPasswordAsync valida o token criptografico e atualiza a senha
        // A nova senha passa pelas validacoes configuradas no Identity
        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

        // Se a redefinicao falhar, lanca excecao com os erros
        if (!result.Succeeded)
        {
            // Concatena todos os erros do Identity
            var errors = string.Join("; ",
                result.Errors.Select(e => e.Description));

            // Lanca excecao com os detalhes
            throw new DomainException($"Falha ao redefinir senha: {errors}");
        }

        // Registra sucesso no log
        _logger.LogInformation("Senha redefinida com sucesso para usuario: {UserId}", user.Id);
    }

    // Caso de uso: Logout (invalida o refresh token)
    // Fluxo simples: revoga o refresh token para que nao possa mais ser usado
    // Operacao idempotente: se o token ja foi revogado, nao lanca erro
    public Task LogoutAsync(string refreshToken)
    {
        // Registra o logout no log
        _logger.LogInformation("Logout: revogando refresh token");

        // Revoga o refresh token no servico de infraestrutura
        // RevokeToken remove o token do ConcurrentDictionary
        // Se o token ja nao existir, a operacao simplesmente nao faz nada
        _refreshTokenService.RevokeToken(refreshToken);

        // Retorna Task.CompletedTask pois e uma operacao sincrona
        // Em uma implementacao com banco de dados, seria await repository.RevokeAsync(...)
        return Task.CompletedTask;
    }
}
