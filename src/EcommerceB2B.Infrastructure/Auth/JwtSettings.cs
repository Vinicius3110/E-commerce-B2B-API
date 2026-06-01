// Namespace que organiza as classes relacionadas à autenticação JWT
// dentro da camada de infraestrutura
namespace EcommerceB2B.Infrastructure.Auth;

// Classe de configuração fortemente tipada para os parâmetros do JWT
// Será preenchida a partir do appsettings.json via options pattern do .NET
// O options pattern permite vincular seções do JSON a objetos C# automaticamente
// através de services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))
public class JwtSettings
{
    // Chave secreta usada para assinar o token JWT
    // Deve ser uma string longa e aleatória em produção (mínimo 32 caracteres)
    // A assinatura garante que o token não foi adulterado entre o servidor e o cliente
    public string Secret { get; set; } = string.Empty;

    // Emissor (issuer) do token — identifica quem gerou o token
    // Geralmente é o nome/URL da API
    // O cliente valida que o token foi emitido por uma fonte confiável
    public string Issuer { get; set; } = string.Empty;

    // Audiência (audience) — identifica para quem o token é destinado
    // Pode ser o nome da aplicação cliente ou uma lista de destinatários
    // O cliente valida que o token foi realmente destinado a ele
    public string Audience { get; set; } = string.Empty;

    // Tempo de expiração do access token em minutos
    // Recomendação: 15-60 minutos para access token em produção
    // Tokens de curta duração reduzem o risco em caso de roubo do token
    public int ExpirationMinutes { get; set; } = 60;

    // Tempo de expiração do refresh token em dias
    // Refresh token dura mais porque permite renovar o access token sem login
    // Em caso de comprometimento, o refresh token pode ser revogado pelo servidor
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
