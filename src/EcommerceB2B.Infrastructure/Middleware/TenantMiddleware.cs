// Importa o HttpContext para acessar a requisição, resposta e usuário autenticado
using Microsoft.AspNetCore.Http;

// Importa ClaimsPrincipal para acessar as claims do usuário autenticado no JWT
using System.Security.Claims;

// Namespace que organiza os middlewares personalizados da aplicação
namespace EcommerceB2B.Infrastructure.Middleware;

// Middleware de isolamento multi-tenant
// Extrai o CompanyId da claim "company_id" do JWT e o disponibiliza em HttpContext.Items
// Isso permite que todas as camadas subsequentes acessem o CompanyId sem precisar
// decodificar o JWT novamente — basta ler context.Items["CompanyId"]
//
// Pipeline de execução:
//   Request → TenantMiddleware → next middleware → ... → Response
//
// Registro no Program.cs:
//   app.UseMiddleware<TenantMiddleware>();
public class TenantMiddleware
{
    // Delegado que representa o próximo middleware no pipeline
    // readonly garante que a referência não será alterada
    // Invocar _next(context) passa o controle para o próximo middleware
    private readonly RequestDelegate _next;

    // Construtor que recebe o próximo middleware via injeção de dependência
    // O RequestDelegate é fornecido automaticamente pelo pipeline do ASP.NET Core
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Método principal do middleware — chamado a cada requisição HTTP
    // O ASP.NET Core chama este método automaticamente quando uma requisição chega
    // Parâmetros:
    //   context: HttpContext contendo todas as informações da requisição atual
    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se o usuário está autenticado na requisição atual
        // Identity?.IsAuthenticated é true quando o JWT foi validado com sucesso
        // pelo middleware de autenticação (UseAuthentication)
        // Isso evita tentar extrair claims de requisições anônimas (login, registro, etc.)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extrai o valor da claim "company_id" do token JWT do usuário
            // FindFirst busca a primeira claim com o tipo especificado
            // A claim "company_id" foi inserida no token pelo JwtService.GenerateToken
            var companyIdClaim = context.User.FindFirst("company_id")?.Value;

            // Verifica se a claim existe e tenta converter para Guid
            // IsNullOrEmpty evita tentar parsear uma string vazia
            // Guid.TryParse retorna true se a conversão for bem-sucedida
            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
            {
                // Armazena o CompanyId no dicionário Items do HttpContext
                // Items é um dicionário que dura apenas o escopo da requisição
                // Perfeito para compartilhar dados entre middlewares e controllers
                // Chave "CompanyId": string constante usada para recuperar o valor depois
                context.Items["CompanyId"] = companyId;
            }
        }

        // Sempre chama o próximo middleware no pipeline
        // Isso é obrigatório — se não chamar _next, a requisição não avança
        // e o cliente nunca recebe uma resposta
        await _next(context);
    }
}
