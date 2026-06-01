using System.IdentityModel.Tokens.Jwt;

// Importa a interface IOptions para acessar as configurações via options pattern
using Microsoft.Extensions.Options;

// Importa os tipos de segurança do IdentityModel para criar e validar tokens
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

// Namespace que organiza as classes relacionadas à autenticação JWT
namespace EcommerceB2B.Infrastructure.Auth;

// Serviço responsável pela geração e validação de tokens JWT
// Registrado como Singleton no contêiner DI — apenas uma instância para toda a aplicação
// Isso é seguro porque o serviço não mantém estado mutável, apenas lê configurações
public class JwtService
{
    // Armazena as configurações JWT carregadas do appsettings.json
    // readonly garante que a referência não será alterada após a construção
    // IOptions<JwtSettings> fornece acesso ao valor configurado de JwtSettings
    private readonly JwtSettings _jwtSettings;

    // Construtor que recebe as configurações JWT via injeção de dependência
    // IOptions<JwtSettings> é registrado automaticamente quando chamamos
    // services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))
    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        // Obtém o valor da configuração (objeto JwtSettings preenchido)
        _jwtSettings = jwtSettings.Value;
    }

    // Gera um token JWT para um usuário autenticado
    // Parâmetros:
    //   userId: ID único do usuário que está se autenticando
    //   companyId: ID da empresa à qual o usuário pertence (multi-tenant)
    //   role: papel/função do usuário no sistema (ex: "Admin", "Buyer", "Seller")
    //   email: email do usuário para identificação adicional
    // Retorna o token JWT como string codificada em Base64
    public string GenerateToken(Guid userId, Guid companyId, string role, string email)
    {
        // Cria a chave de segurança simétrica a partir do segredo configurado
        // Encoding.UTF8.GetBytes converte a string secreta em um array de bytes
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        // Cria as credenciais de assinatura usando o algoritmo HMAC-SHA256
        // HMAC (Hash-based Message Authentication Code) garante integridade e autenticidade
        // O algoritmo SHA256 é um padrão seguro e amplamente aceito para assinatura JWT
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Constrói a lista de claims (declarações) que serão incluídas no token
        var claims = new List<Claim>
        {
            // sub (subject): identificador único do usuário — claim padrão JWT
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),

            // email: endereço de email do usuário — claim padrão JWT
            new Claim(JwtRegisteredClaimNames.Email, email),

            // jti (JWT ID): identificador único do token — útil para revogação
            // Cada token gerado terá um JTI diferente, permitindo blacklist se necessário
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // iat (issued at): timestamp Unix de quando o token foi emitido
            // DateTimeOffset.UtcNow.ToUnixTimeSeconds() retorna segundos desde 1970-01-01
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            // company_id: claim customizada para identificar a empresa no sistema multi-tenant
            // O TenantMiddleware extrai esta claim para determinar o contexto da empresa
            new Claim("company_id", companyId.ToString()),

            // role: função do usuário no sistema — usada para autorização
            // Ex: [Authorize(Roles = "Admin")] verificará esta claim
            new Claim(ClaimTypes.Role, role)
        };

        // Cria o objeto JwtSecurityToken com todas as configurações
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,                             // Emissor do token
            audience: _jwtSettings.Audience,                         // Destinatário do token
            claims: claims,                                           // Lista de claims
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes), // Data de expiração
            signingCredentials: credentials);                        // Credenciais de assinatura

        // Serializa o token para string JWT no formato: header.payload.signature
        // O resultado é uma string Base64URL que pode ser enviada no header Authorization
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    // Valida um token JWT e extrai as claims se o token for válido
    // Parâmetros:
    //   token: string JWT a ser validada
    // Retorna:
    //   ClaimsPrincipal com as claims do usuário se o token for válido
    //   null se o token for inválido, expirado ou adulterado
    public ClaimsPrincipal? ValidateToken(string token)
    {
        // Cria o handler que processará a validação do token
        var tokenHandler = new JwtSecurityTokenHandler();

        // Define os parâmetros de validação do token
        // Configura validações rigorosas para garantir segurança máxima
        var validationParameters = new TokenValidationParameters
        {
            // Valida se o emissor do token corresponde ao configurado
            // Impede que tokens emitidos por outras aplicações sejam aceitos
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,

            // Valida se a audiência do token corresponde à configurada
            // Impede que tokens destinados a outras aplicações sejam aceitos
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,

            // Valida a assinatura do token usando a chave secreta
            // Impede que tokens adulterados ou forjados sejam aceitos
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),

            // ClockSkew = TimeSpan.Zero elimina a tolerância de 5 minutos padrão
            // Sem isso, tokens expirados seriam aceitos por até 5 minutos a mais
            // Em produção, um pequeno ClockSkew (ex: 30s) pode ser útil para
            // compensar diferenças de relógio entre servidores
            ClockSkew = TimeSpan.Zero,

            // Valida o tempo de expiração (exp) do token
            // Tokens expirados são rejeitados automaticamente
            ValidateLifetime = true
        };

        try
        {
            // Tenta validar o token e extrair o principal (claims do usuário)
            // O parâmetro out _ descarta a exceção de segurança (já tratamos via try/catch)
            // O parâmetro validatedToken não é necessário aqui, então descartamos
            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            // Retorna o principal contendo todas as claims do usuário
            return principal;
        }
        catch
        {
            // Se qualquer validação falhar (token expirado, assinatura inválida,
            // emissor inválido, audiência inválida, formato incorreto, etc.)
            // retornamos null indicando que o token não é válido
            // O chamador deve tratar null como "não autorizado"
            return null;
        }
    }
}
