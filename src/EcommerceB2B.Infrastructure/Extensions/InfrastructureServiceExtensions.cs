// Importa as classes de autenticação JWT da camada de infraestrutura
using EcommerceB2B.Infrastructure.Auth;

// Importa o contexto do banco de dados (AppDbContext)
using EcommerceB2B.Infrastructure.Persistence;

// Importa as implementações de repositórios
using EcommerceB2B.Infrastructure.Repositories;

// Importa as interfaces (contratos) de repositórios da camada de domínio
using EcommerceB2B.Domain.Interfaces;

// Importa o ASP.NET Core Identity para configurar autenticação
using Microsoft.AspNetCore.Identity;

// Importa o Entity Framework Core para configurar o banco de dados
using Microsoft.EntityFrameworkCore;

// Importa as interfaces de configuração
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Namespace que organiza as classes de extensão de serviço
namespace EcommerceB2B.Infrastructure.Extensions;

// Classe estática que contém métodos de extensão para IServiceCollection
// Métodos de extensão permitem adicionar funcionalidades a interfaces existentes
// sem modificar a interface original — princípio Aberto/Fechado (OCP)
// Esta classe centraliza todo o registro de DI da camada de infraestrutura
public static class InfrastructureServiceExtensions
{
    // Método de extensão que registra todos os serviços da camada de infraestrutura
    // Parâmetros:
    //   services: coleção de serviços do ASP.NET Core (this indica método de extensão)
    //   configuration: acesso às configurações do appsettings.json
    // Retorna:
    //   IServiceCollection para permitir encadeamento de chamadas (fluent pattern)
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─────────────────────────────────────────────────────────
        // 1. Configuração do Banco de Dados (PostgreSQL)
        // ─────────────────────────────────────────────────────────

        // Registra o AppDbContext no contêiner DI com provedor PostgreSQL
        // UseNpgsql configura o EF Core para usar o driver Npgsql
        // GetConnectionString("DefaultConnection") busca a string de conexão no appsettings.json
        // O ciclo de vida padrão do DbContext é Scoped (uma instância por requisição)
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // ─────────────────────────────────────────────────────────
        // 2. Configuração do ASP.NET Core Identity
        // ─────────────────────────────────────────────────────────

        // Registra o Identity com tipos personalizados usando Guid como chave primária
        // IdentityUser<Guid>: usuário com ID Guid (em vez do padrão string)
        // IdentityRole<Guid>: role/perfil com ID Guid
        // O Identity gerencia: criação de usuários, senhas, roles, claims, tokens
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
        {
            // ── Configurações de SignIn (login) ──

            // Exige que o email do usuário seja confirmado antes de permitir login
            // O fluxo: registro → email de confirmação → clique no link → login liberado
            // Isso impede que bots criem contas com emails falsos
            options.SignIn.RequireConfirmedEmail = true;

            // ── Configurações de Senha ──

            // Exige pelo menos 1 dígito numérico (0-9) na senha
            options.Password.RequireDigit = true;

            // Define o comprimento mínimo da senha como 8 caracteres
            // Padrão do Identity é 6 — aumentamos para 8 por segurança
            options.Password.RequiredLength = 8;

            // Exige pelo menos 1 caractere não alfanumérico (!@#$%^&* etc.)
            options.Password.RequireNonAlphanumeric = true;

            // Exige pelo menos 1 letra maiúscula (A-Z)
            options.Password.RequireUppercase = true;

            // Exige pelo menos 1 letra minúscula (a-z)
            options.Password.RequireLowercase = true;

            // ── Configurações de Lockout (bloqueio por tentativas) ──

            // Tempo de bloqueio da conta após exceder o limite de tentativas
            // 15 minutos impede ataques de força bruta mantendo usabilidade
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            // Número máximo de tentativas de login com senha incorreta antes do bloqueio
            // 5 tentativas é um bom equilíbrio entre segurança e usabilidade
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        // Adiciona os stores do Entity Framework para persistir os dados do Identity
        // Isso registra UserStore, RoleStore, etc. que usam AppDbContext
        .AddEntityFrameworkStores<AppDbContext>()

        // Adiciona os provedores de token padrão do Identity
        // Necessário para gerar tokens de confirmação de email e redefinição de senha
        // Registra: EmailTokenProvider, PhoneTokenProvider, AuthenticatorTokenProvider
        .AddDefaultTokenProviders();

        // ─────────────────────────────────────────────────────────
        // 3. Configuração do JWT Settings via Options Pattern
        // ─────────────────────────────────────────────────────────

        // Vincula a seção "JwtSettings" do appsettings.json à classe JwtSettings
        // Isso preenche automaticamente as propriedades (Secret, Issuer, Audience, etc.)
        // IOptions<JwtSettings> fica disponível para injeção em qualquer classe
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        // ─────────────────────────────────────────────────────────
        // 4. Registro dos Serviços de Autenticação
        // ─────────────────────────────────────────────────────────

        // JwtService: Singleton — thread-safe, sem estado mutável
        // Apenas uma instância para toda a aplicação
        services.AddSingleton<JwtService>();

        // RefreshTokenService: Singleton — ConcurrentDictionary persiste em memória
        // Em produção com múltiplos servidores, usar cache distribuído (Redis)
        services.AddSingleton<RefreshTokenService>();

        // EmailService: Scoped — uma instância por requisição HTTP
        // O logger tipado depende do contexto da requisição
        services.AddScoped<EmailService>();

        // ─────────────────────────────────────────────────────────
        // 5. Registro dos Repositórios
        // ─────────────────────────────────────────────────────────

        // CompanyRepository: Scoped — gerencia empresas (tenants)
        // Implementa ICompanyRepository definido na camada de domínio
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // CategoryRepository: Scoped — gerencia categorias de produtos
        // Implementa ICategoryRepository definido na camada de domínio
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ProductRepository: Scoped — gerencia produtos do marketplace B2B
        // Implementa IProductRepository definido na camada de domínio
        services.AddScoped<IProductRepository, ProductRepository>();

        // OrderRepository: Scoped — gerencia pedidos de compra B2B
        // Implementa IOrderRepository definido na camada de domínio
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ─────────────────────────────────────────────────────────

        // Retorna a coleção de serviços para permitir encadeamento (fluent pattern)
        // Exemplo de uso no Program.cs:
        //   builder.Services.AddInfrastructure(builder.Configuration)
        //                  .AddApplication()
        //                  .AddApi();
        return services;
    }
}
