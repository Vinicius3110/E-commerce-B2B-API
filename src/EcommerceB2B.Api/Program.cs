// Importacoes para os servicos e middlewares que compoem a aplicacao
using EcommerceB2B.Api.Middleware; // ExceptionHandlingMiddleware (captura global de excecoes)
using EcommerceB2B.Application.Interfaces; // Interface IAuthService consumida pelo AuthController
using EcommerceB2B.Application.UseCases.Auth; // Implementacao concreta do servico de autenticacao
using EcommerceB2B.Application.UseCases.Category; // Implementacao concreta do servico de categorias
using EcommerceB2B.Application.UseCases.Company; // Implementacao concreta do servico de empresas
using EcommerceB2B.Application.UseCases.Order; // Implementacao concreta do servico de pedidos
using EcommerceB2B.Application.UseCases.Product; // Implementacao concreta do servico de produtos
using EcommerceB2B.Infrastructure.Auth; // Servicos de autenticacao JWT (JwtService, RefreshTokenService)
using EcommerceB2B.Infrastructure.Extensions; // Metodo de extensao AddInfrastructure (registra todos os servicos)
using EcommerceB2B.Infrastructure.Middleware; // TenantMiddleware (extracao de CompanyId do JWT)
using EcommerceB2B.Infrastructure.Persistence; // AppDbContext (contexto do Entity Framework Core)
using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults e AddJwtBearer para autenticacao JWT
using Microsoft.IdentityModel.Tokens; // SymmetricSecurityKey, TokenValidationParameters para configuracao do JWT
using System.Text; // Encoding.UTF8 para converter a chave secreta em bytes

// Cria o builder da aplicacao web com as configuracoes padrao
// WebApplication.CreateBuilder le appsettings.json, configura logging e DI
var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURACAO DE SERVICOS ==========

// Adiciona suporte a controllers (classes que recebem requisicoes HTTP)
// Registra automaticamente todas as classes com [ApiController] no assembly
builder.Services.AddControllers();

// Configura o Swagger/OpenAPI para documentacao da API
// Util para testar endpoints via interface web durante o desenvolvimento
builder.Services.AddEndpointsApiExplorer(); // Expos metadados dos endpoints para o Swagger gerar a UI
builder.Services.AddSwaggerGen(); // Configura o gerador de documentacao Swagger/OpenAPI

// Registra todos os servicos da camada de Infraestrutura (DbContext, Identity, JWT, repositorios)
// AddInfrastructure e um metodo de extensao que centraliza a configuracao de DI
builder.Services.AddInfrastructure(builder.Configuration);

// Configura a autenticacao JWT no pipeline do ASP.NET Core
// AddAuthentication registra os servicos de autenticacao no contêiner DI
builder.Services.AddAuthentication(options =>
{
    // Define JWT como esquema padrao de autenticacao
    // Toda requisicao que exigir autenticacao usara JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // Define JWT como esquema padrao de desafio (challenge)
    // Quando o usuario nao autenticado tentar acessar recurso protegido,
    // o sistema respondera com 401 usando o esquema JWT
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => // Configura o handler de validacao de tokens JWT
{
    // Le as configuracoes do JWT do appsettings.json (secao JwtSettings)
    // A secao JwtSettings contem Secret, Issuer, Audience, etc.
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");

    // Obtem a chave secreta usada para assinar e validar os tokens
    // O operador ! (null-forgiving) indica que o valor nunca sera nulo
    // pois a secao JwtSettings esta configurada no appsettings.json
    var secret = jwtSettings["Secret"]!;

    // Configura os parametros de validacao do token JWT recebido
    // O middleware de autenticacao aplicara estas regras em toda requisicao
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Valida o emissor (issuer) do token
        // Garante que o token foi gerado por uma fonte confiavel (nossa API)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"], // Emissor valido configurado no appsettings

        // Valida a audiencia (audience) do token
        // Garante que o token foi destinado a esta aplicacao cliente
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"], // Audiencia valida configurada no appsettings

        // Valida a chave de assinatura do token
        // Garante que o token nao foi adulterado (assinatura HMAC-SHA256)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)), // Converte a string secreta em chave criptografica

        // Valida o tempo de expiracao do token
        // Tokens expirados sao rejeitados automaticamente
        ValidateLifetime = true,

        // Sem tolerancia de clock (expiracao exata)
        // ClockSkew Zero significa que o token expira exatamente no horario definido
        // O padrao do .NET e 5 minutos de tolerancia — removemos para mais seguranca
        ClockSkew = TimeSpan.Zero
    };
});

// Registra os servicos da camada de Aplicacao no contêiner de DI
// Scoped = uma nova instancia por requisicao HTTP (ciclo de vida ideal para servicos de negocio)
// Isso garante isolamento entre requisicoes concorrentes

// AuthService: implementa IAuthService — interface para injecao no AuthController
builder.Services.AddScoped<IAuthService, AuthService>();

// CompanyService: gerencia empresas (tenants) e usuarios vinculados
builder.Services.AddScoped<CompanyService>();

// CategoryService: gerencia categorias de produtos (listagem publica, CRUD autenticado)
builder.Services.AddScoped<CategoryService>();

// ProductService: gerencia produtos do marketplace B2B (catalogo, precos)
builder.Services.AddScoped<ProductService>();

// OrderService: gerencia pedidos de compra B2B (criacao, transicao de status)
builder.Services.AddScoped<OrderService>();

// Adiciona politica de CORS (Cross-Origin Resource Sharing)
// Permite que frontends em outros dominios acessem a API
// Necessario quando o frontend (React, Angular, etc.) esta em dominio diferente da API
builder.Services.AddCors(options =>
{
    // Cria uma politica chamada "AllowAll" para desenvolvimento
    // Em producao, restringir as origens permitidas para maior seguranca
    options.AddPolicy("AllowAll", policy =>
    {
        // Permite requisicoes de qualquer origem (dominio)
        // Em producao: substituir por dominios especificos (ex: "https://meuapp.com")
        policy.AllowAnyOrigin()
              // Permite qualquer metodo HTTP (GET, POST, PUT, DELETE, PATCH, etc.)
              .AllowAnyMethod()
              // Permite qualquer header HTTP na requisicao
              .AllowAnyHeader();
    });
});

// ========== CONSTRUCAO DO APP ==========

// Build() compila a aplicacao com todas as configuracoes de servicos
// A partir deste ponto, o pipeline de middlewares e configurado
var app = builder.Build();

// ========== CONFIGURACAO DO PIPELINE DE MIDDLEWARE ==========

// Middleware de tratamento global de excecoes — DEVE ser o PRIMEIRO middleware
// Ele envolve todo o pipeline subsequente em um try/catch
// Qualquer excecao nao tratada em middlewares posteriores sera capturada aqui
// e transformada em resposta JSON padronizada (400, 403 ou 500)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger disponivel apenas em ambiente de desenvolvimento
// Em producao, a documentacao da API nao fica exposta publicamente
if (app.Environment.IsDevelopment())
{
    // Middleware que serve a pagina Swagger UI (interface grafica interativa)
    app.UseSwagger();

    // Middleware que configura o endpoint da UI do Swagger
    // Acessivel em /swagger — exibe todos os endpoints documentados
    app.UseSwaggerUI();
}

// Habilita CORS com a politica "AllowAll" definida acima
// Este middleware adiciona os headers CORS nas respostas HTTP
app.UseCors("AllowAll");

// Adiciona o middleware de autenticacao JWT ao pipeline
// Valida o token JWT em cada requisicao que exige autenticacao
// Preenche HttpContext.User com as claims do token validado
app.UseAuthentication();

// Adiciona o middleware de autorizacao (verifica roles/claims)
// Baseado nos atributos [Authorize] e [Authorize(Roles="...")] nos controllers
app.UseAuthorization();

// Middleware de tenant — extrai CompanyId da claim "company_id" do JWT
// e o disponibiliza via HttpContext.Items["CompanyId"]
// Deve vir APOS UseAuthentication e ANTES de MapControllers
app.UseMiddleware<TenantMiddleware>();

// Mapeia os controllers para suas rotas ([Route], [HttpGet], [HttpPost], etc.)
// O roteamento usa reflection para encontrar metodos com atributos de rota
app.MapControllers();

// ========== INICIALIZACAO DO BANCO DE DADOS ==========

// Cria um escopo para resolver servicos Scoped (como DbContext) no startup
// Fora de um escopo, nao e possivel resolver servicos Scoped
// O bloco using garante que o escopo seja descartado apos o uso
using (var scope = app.Services.CreateScope())
{
    // Resolve o AppDbContext do contêiner DI dentro do escopo
    // GetRequiredService lanca excecao se o servico nao estiver registrado
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // EnsureCreatedAsync garante que o banco de dados e todas as tabelas existam
    // Cria o banco se nao existir — util para desenvolvimento
    // Em producao, usar Migrations ao inves de EnsureCreatedAsync
    // Migrations permitem controle de versao do esquema do banco
    await dbContext.Database.EnsureCreatedAsync();
}

// Inicia a aplicacao e comeca a escutar requisicoes HTTP
// Run() bloqueia a thread atual ate que o servidor seja encerrado
app.Run();
