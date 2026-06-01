using System.Collections.Concurrent;

// Namespace que organiza as classes relacionadas à autenticação
namespace EcommerceB2B.Infrastructure.Auth;

// Entrada de refresh token armazenada no dicionário em memória
// Cada entrada contém o token, o usuário dono, a empresa e a data de expiração
// Registrada como Singleton, então o ConcurrentDictionary persiste durante
// toda a vida útil da aplicação
public class RefreshTokenEntry
{
    // Token de refresh — string única gerada via Guid
    // O cliente envia este token para obter um novo access token sem fazer login
    public string Token { get; set; } = string.Empty;

    // ID do usuário dono do refresh token
    // Usado para identificar qual usuário está solicitando a renovação
    public Guid UserId { get; set; }

    // ID da empresa associada ao usuário no momento da geração do token
    // Necessário para manter o contexto multi-tenant na renovação
    public Guid CompanyId { get; set; }

    // Data e hora de expiração do refresh token em UTC
    // Após esta data, o token não pode mais ser usado para renovação
    public DateTime ExpiresAt { get; set; }
}

// Serviço de gerenciamento de refresh tokens
// Implementação em memória usando ConcurrentDictionary — para fins de estudo
// Em produção, os refresh tokens devem ser armazenados em banco de dados
// ou cache distribuído (Redis) para persistência e escalabilidade
// ConcurrentDictionary é thread-safe, permitindo acesso concorrente sem locks manuais
public class RefreshTokenService
{
    // Dicionário thread-safe que armazena os refresh tokens em memória
    // Chave: string do token (Guid sem hífens)
    // Valor: RefreshTokenEntry com metadados do token
    private static readonly ConcurrentDictionary<string, RefreshTokenEntry> _refreshTokens = new();

    // Gera um novo refresh token para o usuário
    // Parâmetros:
    //   userId: ID do usuário que está recebendo o refresh token
    //   companyId: ID da empresa do usuário (contexto multi-tenant)
    // Retorna:
    //   Tuple com o token (string) e a data de expiração (DateTime)
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid companyId)
    {
        // Gera um token único usando Guid.NewGuid() sem hífens (formato "N")
        // O formato "N" produz 32 caracteres hexadecimais, mais compacto que o formato "D"
        var token = Guid.NewGuid().ToString("N");

        // Calcula a data de expiração com base na configuração
        // O RefreshTokenExpirationDays define quantos dias o token é válido
        // Usamos DateTime.UtcNow para consistência de fuso horário
        var expiresAt = DateTime.UtcNow.AddDays(7); // 7 dias de validade

        // Cria a entrada do refresh token com todos os metadados
        var entry = new RefreshTokenEntry
        {
            Token = token,
            UserId = userId,
            CompanyId = companyId,
            ExpiresAt = expiresAt
        };

        // Adiciona ao dicionário em memória de forma thread-safe
        // TryAdd retorna false se a chave já existir (colisão extremamente rara com Guids)
        // O operador de índice ( _refreshTokens[token] = entry ) também funciona
        // mas TryAdd é mais seguro e explícito sobre a intenção
        _refreshTokens.TryAdd(token, entry);

        // Retorna o token e a data de expiração para o chamador
        return (token, expiresAt);
    }

    // Valida um refresh token e retorna seus metadados se for válido
    // Parâmetros:
    //   token: string do refresh token a ser validado
    // Retorna:
    //   RefreshTokenEntry se o token existir e não estiver expirado
    //   null se o token não existir ou estiver expirado
    public RefreshTokenEntry? ValidateAndGetRefreshToken(string token)
    {
        // Tenta encontrar o token no dicionário
        if (!_refreshTokens.TryGetValue(token, out var entry))
        {
            // Token não encontrado — nunca foi gerado ou já foi revogado
            return null;
        }

        // Verifica se o token já expirou
        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            // Token expirado: remove do dicionário para evitar acúmulo de lixo
            // TryRemove é thread-safe e remove apenas se a chave existir
            _refreshTokens.TryRemove(token, out _);

            // Retorna null indicando que o token não é mais válido
            return null;
        }

        // Token encontrado e dentro da validade: retorna os metadados
        // O chamador pode usar UserId e CompanyId para gerar um novo access token
        return entry;
    }

    // Revoga (remove) um refresh token específico
    // Útil para logout e para revogar tokens comprometidos
    // Parâmetros:
    //   token: string do refresh token a ser revogado
    public void RevokeToken(string token)
    {
        // Remove o token do dicionário de forma thread-safe
        // Se o token não existir, a operação simplesmente não faz nada
        _refreshTokens.TryRemove(token, out _);
    }
}
