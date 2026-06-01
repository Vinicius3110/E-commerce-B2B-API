// Importa o DTO de paginacao generica (PaginatedResult<T>) usado como retorno da listagem
using EcommerceB2B.Application.DTOs.Common;

// Importa os DTOs de produto usados como entrada e saida dos endpoints
using EcommerceB2B.Application.DTOs.Product;

// Importa o servico de produto que orquestra as operacoes de CRUD
using EcommerceB2B.Application.UseCases.Product;

// Importa o ASP.NET Core MVC para atributos dos endpoints
using Microsoft.AspNetCore.Authorization; // [Authorize] para restringir acesso
using Microsoft.AspNetCore.Mvc; // ControllerBase, IActionResult, atributos HTTP

// Namespace que organiza os controllers da API REST
namespace EcommerceB2B.Api.Controllers;

// Controller responsavel pelos endpoints de gerenciamento de produtos do marketplace B2B
// [ApiController] ativa validacao automatica do ModelState e binding de parametros
// [Authorize] no nivel da classe: TODOS os endpoints exigem autenticacao JWT
//   O usuario precisa ter um token JWT valido para acessar qualquer endpoint
// [Route("api/products")] define o prefixo de rota para todos os endpoints
[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    // Servico de produto injetado via DI
    // Orquestra operacoes: listagem paginada, detalhes, criacao, atualizacao, desativacao e precos
    private readonly ProductService _productService;

    // Construtor que recebe o ProductService por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao automaticamente
    public ProductsController(ProductService productService)
    {
        // Armazena a referencia do servico de produtos
        _productService = productService;
    }

    // Metodo auxiliar privado: extrai o CompanyId do HttpContext.Items
    // O CompanyId foi armazenado pelo TenantMiddleware a partir da claim "company_id" do JWT
    // Usado para associar o produto a empresa vendedora (quem criou o produto)
    // Alem disso, valida permissoes: apenas a empresa dona pode editar/desativar seu produto
    // Se o CompanyId nao existir no Items, lanca UnauthorizedAccessException
    // Retorna:
    //   Guid: identificador unico da empresa do usuario autenticado
    private Guid GetCompanyId()
    {
        // Tenta obter o CompanyId do dicionario Items do HttpContext
        // Items e populado pelo TenantMiddleware durante a autenticacao
        if (HttpContext.Items.TryGetValue("CompanyId", out var companyIdObj) &&
            companyIdObj is Guid companyId) // Pattern matching: verifica o tipo Guid
        {
            // Retorna o CompanyId da empresa autenticada
            return companyId;
        }

        // Se nao encontrou CompanyId, lanca excecao de autorizacao
        // O middleware ExceptionHandlingMiddleware captura e retorna 403 Forbidden
        throw new UnauthorizedAccessException("Empresa nao identificada no token.");
    }

    // GET api/products
    // Endpoint autenticado para listar produtos com filtros e paginacao
    // Filtros opcionais passados via query string:
    //   categoryId: filtra produtos de uma categoria especifica
    //   sellerCompanyId: filtra produtos de um fornecedor especifico
    //   minPrice: preco minimo dos produtos
    //   maxPrice: preco maximo dos produtos
    //   page: numero da pagina (default: 1)
    //   pageSize: quantidade de itens por pagina (default: 20)
    // [FromQuery] vincula parametros da query string (ex: ?page=1&pageSize=10)
    // Retorna:
    //   200 OK com PaginatedResult<ProductDto> contendo itens e metadados de paginacao
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)] // Documenta sucesso
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId = null, // Filtro opcional por categoria
        [FromQuery] Guid? sellerCompanyId = null, // Filtro opcional por fornecedor
        [FromQuery] decimal? minPrice = null, // Filtro opcional por preco minimo
        [FromQuery] decimal? maxPrice = null, // Filtro opcional por preco maximo
        [FromQuery] int page = 1, // Pagina atual (default 1)
        [FromQuery] int pageSize = 20, // Tamanho da pagina (default 20)
        CancellationToken cancellationToken = default)
    {
        // Chama o servico de produto com todos os filtros
        // GetFilteredAsync aplica cada filtro se o valor nao for nulo
        // Retorna PaginatedResult com items, totalCount, metadados de navegacao
        var result = await _productService.GetFilteredAsync(
            categoryId,
            sellerCompanyId,
            minPrice,
            maxPrice,
            page,
            pageSize,
            cancellationToken);

        // Ok() retorna 200 OK com o PaginatedResult no corpo JSON
        return Ok(result);
    }

    // GET api/products/{id}
    // Endpoint autenticado para obter detalhes completos de um produto
    // Inclui propriedades de navegacao: nome da empresa vendedora e nome da categoria
    // Parametros:
    //   id: identificador unico do produto (Guid)
    // Retorna:
    //   200 OK com ProductDto contendo todos os detalhes
    //   400 Bad Request se o produto nao for encontrado
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta nao encontrado
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Chama o servico de produto para buscar pelo ID com detalhes
        // GetByIdAsync com includeDetails: true carrega Company e Category (navegacao)
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        // Ok() retorna 200 OK com o ProductDto no corpo JSON
        return Ok(product);
    }

    // POST api/products
    // Endpoint autenticado para criar um novo produto no catalogo da empresa vendedora
    // O CompanyId e extraido do JWT automaticamente (nao vem no corpo da requisicao)
    // Fluxo:
    //   1. Extrai CompanyId do token JWT (empresa vendedora = empresa autenticada)
    //   2. Verifica unicidade do SKU para esta empresa
    //   3. Cria o produto com os dados recebidos
    //   4. Retorna o DTO do produto criado
    // Retorna:
    //   201 Created com ProductDto
    //   400 Bad Request se SKU duplicado ou dados invalidos
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)] // Documenta criacao
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta dados invalidos
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto request,
        CancellationToken cancellationToken)
    {
        // Obtem o CompanyId do JWT via TenantMiddleware
        // A empresa autenticada sera a vendedora do produto
        var companyId = GetCompanyId();

        // Chama o servico de produto para criar o novo produto
        // CreateAsync passa companyId + dados do DTO, retorna ProductDto com ID gerado
        var product = await _productService.CreateAsync(companyId, request, cancellationToken);

        // CreatedAtAction retorna 201 Created com localizacao do recurso
        // nameof(GetById): referencia o endpoint GET por ID para o header Location
        // routeValues: parametro id para construir a URL → /api/products/{id}
        // value: DTO do produto criado no corpo da resposta
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/{id}
    // Endpoint autenticado para atualizar um produto existente
    // Apenas a empresa dona do produto (CompanyId do JWT = CompanyId do produto) pode editar
    // Fluxo:
    //   1. Extrai CompanyId do JWT
    //   2. Busca produto, verifica ownership (CompanyId do produto == CompanyId do JWT)
    //   3. Verifica unicidade do novo SKU (excluindo o proprio produto)
    //   4. Aplica alteracoes e persiste
    // Retorna:
    //   200 OK com ProductDto atualizado
    //   400 Bad Request se produto nao encontrado, SKU duplicado ou sem permissao
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> Update(
        Guid id, // ID do produto a ser atualizado (da URL)
        [FromBody] UpdateProductDto request, // Novos dados do produto (do corpo JSON)
        CancellationToken cancellationToken)
    {
        // Obtem o CompanyId do JWT para verificacao de ownership
        var companyId = GetCompanyId();

        // Chama o servico de produto para atualizar
        // UpdateAsync verifica ownership: apenas a empresa dona pode editar
        var product = await _productService.UpdateAsync(id, request, companyId, cancellationToken);

        // Ok() retorna 200 OK com o DTO do produto atualizado
        return Ok(product);
    }

    // DELETE api/products/{id}
    // Endpoint autenticado para desativar um produto (soft delete)
    // Soft delete: o registro permanece no banco mas IsActive = false
    //   O produto nao aparece mais em buscas e listagens
    // Apenas a empresa dona do produto pode desativa-lo
    // Retorna:
    //   204 No Content em caso de sucesso (sem corpo na resposta)
    //   400 Bad Request se produto nao encontrado ou sem permissao
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Documenta sucesso sem corpo
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> Delete(
        Guid id, // ID do produto a ser desativado
        CancellationToken cancellationToken)
    {
        // Obtem o CompanyId do JWT para verificacao de ownership
        var companyId = GetCompanyId();

        // Chama o servico de produto para desativar (soft delete)
        // DeactivateAsync verifica ownership e define IsActive = false
        await _productService.DeactivateAsync(id, companyId, cancellationToken);

        // NoContent() retorna 204 No Content — DELETE bem-sucedido, sem corpo
        return NoContent();
    }

    // POST api/products/{id}/prices
    // Endpoint autenticado para definir um preco customizado para um comprador especifico
    // Modelo B2B: fornecedores podem oferecer precos diferentes por cliente
    // Vincula o preco a uma quantidade minima de compra (ex: "acima de 100 unidades, preco X")
    // Fluxo:
    //   1. Recebe productId da URL e dados do preco (comprador, valor, qtd minima) do corpo
    //   2. Verifica se o produto existe
    //   3. Cria o registro ProductPrice e persiste
    // Retorna:
    //   200 OK — preco customizado definido com sucesso
    //   400 Bad Request se produto nao encontrado ou dados invalidos
    [HttpPost("{id:guid}/prices")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    public async Task<IActionResult> SetCustomPrice(
        Guid id, // ID do produto (da URL)
        [FromBody] CreateProductPriceDto request, // Dados do preco customizado (do corpo JSON)
        CancellationToken cancellationToken)
    {
        // Chama o servico de produto para definir o preco customizado
        // SetCustomPriceAsync cria ProductPrice vinculando produto, comprador, valor e qtd minima
        await _productService.SetCustomPriceAsync(id, request, cancellationToken);

        // Ok() retorna 200 OK — preco customizado registrado com sucesso
        return Ok();
    }
}
