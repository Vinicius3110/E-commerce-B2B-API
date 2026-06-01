// Importa os DTOs de categoria usados como entrada e saida dos endpoints
using EcommerceB2B.Application.DTOs.Category;

// Importa o servico de categoria que orquestra as operacoes de CRUD
using EcommerceB2B.Application.UseCases.Category;

// Importa o ASP.NET Core MVC para atributos dos endpoints
using Microsoft.AspNetCore.Authorization; // [Authorize] para restringir acesso
using Microsoft.AspNetCore.Mvc; // ControllerBase, IActionResult, atributos HTTP

// Namespace que organiza os controllers da API REST
namespace EcommerceB2B.Api.Controllers;

// Controller responsavel pelos endpoints de gerenciamento de categorias de produtos
// [ApiController] ativa validacao automatica do ModelState e binding de parametros
// [Route("api/categories")] define o prefixo de rota para todos os endpoints
// NAO possui [Authorize] no nivel da classe — endpoints de leitura sao publicos
// Endpoints de escrita (POST, PUT) sao protegidos individualmente com [Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    // Servico de categoria injetado via DI
    // Orquestra operacoes: listagem, criacao e atualizacao de categorias
    private readonly CategoryService _categoryService;

    // Construtor que recebe o CategoryService por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao automaticamente
    public CategoriesController(CategoryService categoryService)
    {
        // Armazena a referencia do servico de categorias
        _categoryService = categoryService;
    }

    // GET api/categories
    // Endpoint PUBLICO para listar todas as categorias ativas
    // Nao exige autenticacao — qualquer visitante pode ver o catalogo de categorias
    // Util para montar menus de navegacao, filtros de produtos e vitrines
    // Suporta cancelamento via CancellationToken (cliente desconectar → operacao cancelada)
    // Retorna:
    //   200 OK com lista de CategoryDto (id, nome, descricao, status ativo)
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)] // Documenta sucesso
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        // Chama o servico de categoria para buscar todas as categorias ativas
        // GetAllAsync retorna apenas categorias com IsActive = true
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        // Ok() retorna 200 OK com a lista de CategoryDto no corpo JSON
        return Ok(categories);
    }

    // POST api/categories
    // Endpoint AUTENTICADO para criar uma nova categoria de produtos
    // Apenas usuarios logados podem criar categorias
    // Fluxo:
    //   1. Recebe nome e descricao opcional no corpo da requisicao
    //   2. Cria a entidade Category (IsActive = true por padrao)
    //   3. Retorna a categoria criada com o ID gerado
    // Retorna:
    //   201 Created com CategoryDto
    //   400 Bad Request se dados invalidos ou nome vazio
    //   401 Unauthorized se token JWT ausente
    [HttpPost]
    [Authorize] // Protege apenas este endpoint — exige autenticacao JWT
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)] // Documenta criacao
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta dados invalidos
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta nao autenticado
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto request,
        CancellationToken cancellationToken)
    {
        // Chama o servico de categoria para criar a nova categoria
        // CreateAsync valida os dados, cria a entidade e persiste no banco
        var category = await _categoryService.CreateAsync(request, cancellationToken);

        // CreatedAtAction retorna 201 Created com localizacao e corpo
        // nameof(GetById): referencia o metodo GET por ID
        // routeValues: { id = category.Id } → parametro da rota do GetById
        // value: category → corpo da resposta com o DTO da categoria criada
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    // GET api/categories/{id}
    // Endpoint PUBLICO para obter uma categoria especifica por ID
    // Nao exige autenticacao — necessario para o CreatedAtAction do Create
    // NOTA: O CategoryService nao tem GetById separado, mas GetAll traz todas ativas
    // Este endpoint busca do resultado geral e filtra por ID
    // Parametros:
    //   id: identificador unico da categoria (Guid)
    // Retorna:
    //   200 OK com CategoryDto se encontrada
    //   404 Not Found se a categoria nao existir ou estiver inativa
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Documenta nao encontrada
    public async Task<IActionResult> GetById(Guid id)
    {
        // Busca todas as categorias ativas do servico
        // Como o servico nao expoe GetById individual, filtramos a lista
        var categories = await _categoryService.GetAllAsync();

        // Usa LINQ FirstOrDefault para encontrar a categoria pelo ID
        // FirstOrDefault retorna o primeiro elemento ou null se nao encontrado
        var category = categories.FirstOrDefault(c => c.Id == id);

        // Se a categoria nao for encontrada (nula), retorna 404 Not Found
        // NotFound() retorna um resultado padronizado com status 404
        if (category is null)
        {
            return NotFound();
        }

        // Ok() retorna 200 OK com o DTO da categoria no corpo JSON
        return Ok(category);
    }

    // PUT api/categories/{id}
    // Endpoint AUTENTICADO para atualizar uma categoria existente
    // Apenas usuarios logados podem modificar categorias
    // Fluxo:
    //   1. Busca a categoria pelo ID
    //   2. Se nao existir, retorna 404
    //   3. Atualiza nome e descricao
    //   4. Persiste as alteracoes no banco
    // Retorna:
    //   200 OK com CategoryDto atualizado
    //   400 Bad Request se dados invalidos
    //   404 Not Found se categoria nao existir
    //   401 Unauthorized se token JWT ausente
    [HttpPut("{id:guid}")]
    [Authorize] // Protege apenas este endpoint — exige autenticacao JWT
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta dados invalidos
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documenta nao autenticado
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Documenta nao encontrada
    public async Task<IActionResult> Update(
        Guid id, // ID da categoria a ser atualizada (da URL)
        [FromBody] UpdateCategoryDto request, // Novos dados da categoria (do corpo JSON)
        CancellationToken cancellationToken)
    {
        // Chama o servico de categoria para atualizar
        // UpdateAsync busca, valida existencia, aplica alteracoes e persiste
        // Se a categoria nao for encontrada, lanca DomainException → middleware retorna 400
        var category = await _categoryService.UpdateAsync(id, request, cancellationToken);

        // Ok() retorna 200 OK com o DTO atualizado no corpo JSON
        return Ok(category);
    }
}
