// Importa os codigos de status HTTP para retornar respostas apropriadas (400, 403, 500)
using System.Net;

// Importa a biblioteca de serializacao JSON para formatar respostas de erro
using System.Text.Json;

// Importa a excecao customizada de dominio para capturar erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Namespace que organiza os middlewares da camada de API
namespace EcommerceB2B.Api.Middleware;

// Middleware global que captura excecoes nao tratadas e retorna respostas JSON padronizadas
// Isso evita que o ASP.NET Core retorne paginas HTML de erro e garante
// que o cliente sempre receba um JSON consistente, independente do tipo de erro
//
// Fluxo de execucao no pipeline:
//   Request → ExceptionHandlingMiddleware → ... (outros middlewares) → Response
//           ↑                                                            |
//           └── Em caso de excecao, intercepta e retorna JSON de erro ───┘
//
// Registro (deve ser registrado em Program.cs):
//   app.UseMiddleware<ExceptionHandlingMiddleware>();
public class ExceptionHandlingMiddleware
{
    // Delegado para o proximo middleware no pipeline de execucao
    // RequestDelegate representa um metodo que processa HttpContext
    // Invocar _next(context) passa o controle para o proximo middleware
    private readonly RequestDelegate _next;

    // Logger para registrar erros no console/arquivo
    // ILogger<T> e o servico de logging padrao do ASP.NET Core
    // O tipo generico <ExceptionHandlingMiddleware> identifica a origem dos logs
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // O construtor recebe o proximo middleware e o logger via injecao de dependencia
    // O ASP.NET Core resolve automaticamente ambos os parametros do contêiner DI
    // RequestDelegate e fornecido pelo pipeline de middlewares
    // ILogger<T> e registrado automaticamente pelo WebApplicationBuilder
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        // Armazena as dependencias nos campos readonly da classe
        _next = next;
        _logger = logger;
    }

    // Metodo invocado em cada requisicao HTTP pelo pipeline do ASP.NET Core
    // O framework chama este metodo automaticamente para cada requisicao
    // O try/catch envolve TODO o pipeline subsequente — qualquer excecao
    // nao tratada sera capturada aqui e transformada em JSON de erro
    // Parametros:
    //   context: HttpContext com todas as informacoes da requisicao e resposta atual
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Tenta executar o proximo middleware no pipeline
            // Se nenhum middleware lancar excecao, a requisicao segue normalmente
            await _next(context);
        }
        catch (DomainException ex)
        {
            // ── Erros de regra de negocio sao ESPERADOS ──
            // DomainException e lancada intencionalmente pelos servicos da aplicacao
            // Exemplos: "Produto nao encontrado", "SKU duplicado", "Empresa nao encontrada"
            // Esses erros sao culpa do cliente (dados invalidos, recurso inexistente)
            // Retornamos 400 Bad Request com a mensagem original

            // Registra como Warning (nao e erro do sistema, e erro do usuario)
            _logger.LogWarning(ex, "Erro de regra de negocio: {Message}", ex.Message);

            // Define o codigo de status HTTP como 400 Bad Request
            // O cliente deve tratar este status e exibir a mensagem de erro
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            // Escreve a resposta JSON com a mensagem de erro
            await WriteErrorResponse(context, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            // ── Erro de autorizacao ──
            // Lancado quando o usuario tenta acessar recurso sem permissao
            // Exemplos: tentar editar produto de outra empresa, acessar sem CompanyId
            // Retornamos 403 Forbidden — acesso negado

            // Registra como Warning (tentativa de acesso nao autorizado)
            _logger.LogWarning(ex, "Acesso nao autorizado: {Message}", ex.Message);

            // Define o codigo de status HTTP como 403 Forbidden
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            // Escreve a resposta JSON com a mensagem de erro
            await WriteErrorResponse(context, ex.Message);
        }
        catch (Exception ex)
        {
            // ── Erro INESPERADO (nao tratado pelo sistema) ──
            // Qualquer excecao que nao seja DomainException ou UnauthorizedAccessException
            // Exemplos: NullReferenceException, SqlException, timeout de rede
            // Por seguranca, a mensagem original NUNCA e exposta ao cliente
            // O cliente recebe uma mensagem generica para nao revelar detalhes internos

            // Registra como Error (falha grave do sistema)
            _logger.LogError(ex, "Erro interno do servidor: {Message}", ex.Message);

            // Define o codigo de status HTTP como 500 Internal Server Error
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Mensagem generica — sem detalhes internos por seguranca
            // Em producao, o cliente nao deve ver stack traces ou SQL
            await WriteErrorResponse(context,
                "Ocorreu um erro interno no servidor. Tente novamente mais tarde.");
        }
    }

    // Metodo auxiliar estatico e privado: escreve a resposta de erro em formato JSON padronizado
    // E static pois nao depende de estado da instancia (apenas de HttpContext e da mensagem)
    // E privado pois e usado apenas por esta classe
    // Parametros:
    //   context: HttpContext da requisicao atual (para escrever a resposta)
    //   message: mensagem de erro a ser enviada ao cliente
    private static async Task WriteErrorResponse(HttpContext context, string message)
    {
        // Define o Content-Type da resposta como JSON com charset UTF-8
        // Isso informa ao cliente que a resposta deve ser interpretada como JSON
        // O charset=utf-8 garante suporte a caracteres acentuados (portugues)
        context.Response.ContentType = "application/json; charset=utf-8";

        // Cria um objeto anonimo com a propriedade "error" contendo a mensagem
        // O formato padronizado permite que o frontend sempre leia response.error
        // independentemente do tipo de erro que ocorreu
        var response = new { error = message };

        // Serializa o objeto anonimo para JSON usando as opcoes padrao
        // JsonSerializer.Serialize converte o objeto C# para string JSON
        // Exemplo de saida: {"error":"Produto nao encontrado."}
        var json = JsonSerializer.Serialize(response);

        // Escreve o JSON no corpo da resposta HTTP
        // WriteAsync e o metodo assincrono padrao para escrever no stream de resposta
        await context.Response.WriteAsync(json);
    }
}
