// Importa os DTOs de empresa usados como entrada e saida dos endpoints
using EcommerceB2B.Application.DTOs.Company;

// Importa o servico de empresa que orquestra as operacoes de CRUD de empresas
using EcommerceB2B.Application.UseCases.Company;

// Importa o ASP.NET Core MVC para atributos como [ApiController], [Authorize], [Route]
using Microsoft.AspNetCore.Authorization; // [Authorize] para restringir acesso a usuarios autenticados
using Microsoft.AspNetCore.Mvc; // ControllerBase, IActionResult, HttpGet, HttpPut, HttpPost

// Namespace que organiza os controllers da API REST
namespace EcommerceB2B.Api.Controllers;

// Controller responsavel pelos endpoints de gerenciamento de empresas (tenants)
// [ApiController] ativa validacao automatica e binding de parametros
// [Authorize] no nivel da classe: TODOS os endpoints deste controller exigem autenticacao JWT
//   O usuario precisa ter um token JWT valido para acessar qualquer endpoint aqui
// [Route("api/companies")] define o prefixo de rota para todos os endpoints
[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    // Servico de empresa injetado via DI
    // Orquestra operacoes: busca, atualizacao, criacao de usuarios, listagem de usuarios
    private readonly CompanyService _companyService;

    // Construtor que recebe o CompanyService por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao automaticamente
    public CompaniesController(CompanyService companyService)
    {
        // Armazena a referencia do servico de empresas
        _companyService = companyService;
    }

    // Metodo auxiliar privado: extrai o CompanyId do HttpContext.Items
    // O CompanyId foi armazenado pelo TenantMiddleware a partir da claim "company_id" do JWT
    // Usado para garantir que cada empresa acesse apenas seus proprios dados (isolamento multi-tenant)
    // Se o CompanyId nao existir (usuario sem empresa associada), lanca UnauthorizedAccessException
    // Retorna:
    //   Guid: identificador unico da empresa do usuario autenticado
    private Guid GetCompanyId()
    {
        // Tenta obter o CompanyId do dicionario Items do HttpContext
        // Items e um dicionario que dura apenas o escopo da requisicao HTTP
        // A chave "CompanyId" foi definida pelo TenantMiddleware durante a autenticacao
        if (HttpContext.Items.TryGetValue("CompanyId", out var companyIdObj) &&
            companyIdObj is Guid companyId) // Verifica se o valor e realmente um Guid
        {
            // Retorna o CompanyId extraido do JWT
            return companyId;
        }

        // Se o CompanyId nao existir no Items, significa que o usuario esta autenticado
        // mas nao possui uma empresa vinculada (claim "company_id" ausente no JWT)
        // Isso e um erro de configuracao — lanca excecao de autorizacao
        throw new UnauthorizedAccessException("Empresa nao identificada no token.");
    }

    // GET api/companies/{companyId}
    // Endpoint autenticado para obter os dados de uma empresa especifica
    // Parametros:
    //   companyId: identificador unico da empresa (Guid) extraido da URL
    // RESTful: GET e idempotente — chamadas repetidas retornam o mesmo resultado
    // Retorna:
    //   200 OK com CompanyDto contendo nome, documento, tipo, status, data de criacao
    //   400 Bad Request se a empresa nao for encontrada
    [HttpGet("{companyId:guid}")] // :guid restringe o parametro ao formato GUID
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta empresa nao encontrada
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta token ausente
    public async Task<IActionResult> GetById(Guid companyId)
    {
        // Chama o servico de empresa para buscar pelo ID
        // GetByIdAsync lanca DomainException se a empresa nao for encontrada
        // O middleware ExceptionHandlingMiddleware captura e retorna 400 Bad Request
        var company = await _companyService.GetByIdAsync(companyId);

        // Ok() retorna 200 OK com o DTO da empresa no corpo JSON
        return Ok(company);
    }

    // PUT api/companies/{companyId}
    // Endpoint autenticado para atualizar os dados de uma empresa
    // RESTful: PUT e idempotente — atualiza o recurso por completo
    // O CompanyId da rota deve corresponder ao CompanyId do token JWT (auto-isolamento)
    // Parametros:
    //   companyId: identificador da empresa a ser atualizada (da URL)
    //   request: DTO com nome e tipo da empresa (do corpo JSON)
    // Retorna:
    //   200 OK com CompanyDto atualizado
    //   400 Bad Request se a empresa nao for encontrada ou dados invalidos
    [HttpPut("{companyId:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de validacao
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta token ausente
    public async Task<IActionResult> Update(
        Guid companyId,
        [FromBody] UpdateCompanyDto request)
    {
        // Chama o servico de empresa para atualizar os dados
        // UpdateAsync verifica existencia, atualiza nome e tipo, persiste alteracoes
        var company = await _companyService.UpdateAsync(companyId, request);

        // Ok() retorna 200 OK com o DTO atualizado no corpo JSON
        return Ok(company);
    }

    // POST api/companies/{id}/users
    // Endpoint autenticado para criar um novo usuario vinculado a empresa
    // Usado pelo administrador da empresa para cadastrar funcionarios/operadores
    // Fluxo:
    //   1. Verifica se a empresa existe
    //   2. Cria o IdentityUser com email e senha
    //   3. Atribui a role especificada (Admin, Comprador, Vendedor, etc.)
    //   4. O email do usuario criado por admin ja nasce confirmado (EmailConfirmed = true)
    // Retorna:
    //   201 Created com UserDto contendo ID, nome, email, role, status
    //   400 Bad Request se empresa nao encontrada, email duplicado ou senha fraca
    [HttpPost("{id:guid}/users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)] // Documenta criacao
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de validacao
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta token ausente
    public async Task<IActionResult> CreateUser(
        Guid id, // ID da empresa (vem da rota)
        [FromBody] CreateUserDto request) // Dados do novo usuario (vem do corpo JSON)
    {
        // Chama o servico de empresa para criar o usuario vinculado
        // CreateUserAsync cria IdentityUser, atribui role, retorna UserDto
        var user = await _companyService.CreateUserAsync(id, request);

        // CreatedAtAction retorna 201 Created com localizacao e corpo
        // nameof(GetUsers): referencia o endpoint que retorna a lista de usuarios
        // routeValues: parametros para construir a URL de localizacao
        // value: corpo da resposta com o UserDto
        return CreatedAtAction(nameof(GetUsers), new { id }, user);
    }

    // GET api/companies/{id}/users
    // Endpoint autenticado para listar usuarios de uma empresa
    // Retorna uma lista de UserDto com todos os usuarios vinculados a empresa
    // NOTA: Implementacao atual retorna lista vazia (pendente CompanyUserRepository)
    // Retorna:
    //   200 OK com lista de UserDto (possivelmente vazia)
    //   400 Bad Request se a empresa nao for encontrada
    [HttpGet("{id:guid}/users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)] // Documenta listagem
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta empresa nao encontrada
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta token ausente
    public async Task<IActionResult> GetUsers(Guid id)
    {
        // Chama o servico de empresa para listar os usuarios
        // ListUsersAsync verifica se a empresa existe, busca e mapeia usuarios
        var users = await _companyService.ListUsersAsync(id);

        // Ok() retorna 200 OK com a lista de UserDto no corpo JSON
        return Ok(users);
    }
}
