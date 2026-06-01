// Importa o DTO de paginacao generica (PaginatedResult<T>) usado como retorno da listagem
using EcommerceB2B.Application.DTOs.Common;

// Importa os DTOs de pedido usados como entrada e saida dos endpoints
using EcommerceB2B.Application.DTOs.Order;

// Importa o servico de pedido que orquestra as operacoes de compra B2B
using EcommerceB2B.Application.UseCases.Order;

// Importa o ASP.NET Core MVC para atributos dos endpoints
using Microsoft.AspNetCore.Authorization; // [Authorize] para restringir acesso
using Microsoft.AspNetCore.Mvc; // ControllerBase, IActionResult, atributos HTTP

// Namespace que organiza os controllers da API REST
namespace EcommerceB2B.Api.Controllers;

// Controller responsavel pelos endpoints de gerenciamento de pedidos de compra B2B
// [ApiController] ativa validacao automatica do ModelState e binding de parametros
// [Authorize] no nivel da classe: TODOS os endpoints exigem autenticacao JWT
//   Apenas usuarios autenticados podem criar, visualizar e gerenciar pedidos
// [Route("api/orders")] define o prefixo de rota para todos os endpoints
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    // Servico de pedido injetado via DI
    // Orquestra operacoes: listagem, detalhes, criacao e transicao de status
    private readonly OrderService _orderService;

    // Construtor que recebe o OrderService por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao automaticamente
    public OrdersController(OrderService orderService)
    {
        // Armazena a referencia do servico de pedidos
        _orderService = orderService;
    }

    // Metodo auxiliar privado: extrai o CompanyId do HttpContext.Items
    // O CompanyId foi armazenado pelo TenantMiddleware a partir da claim "company_id" do JWT
    // Usado para determinar a empresa do usuario autenticado:
    //   - Como COMPRADORA: nos endpoints de criacao de pedido e listagem como buyer
    //   - Como VENDEDORA: nos endpoints de listagem como seller e atualizacao de status
    // Se o CompanyId nao existir no Items, lanca UnauthorizedAccessException
    // Retorna:
    //   Guid: identificador unico da empresa do usuario autenticado
    private Guid GetCompanyId()
    {
        // Tenta obter o CompanyId do dicionario Items do HttpContext
        // Items e populado pelo TenantMiddleware durante a autenticacao
        if (HttpContext.Items.TryGetValue("CompanyId", out var companyIdObj) &&
            companyIdObj is Guid companyId) // Pattern matching para verificar o tipo Guid
        {
            // Retorna o CompanyId da empresa autenticada
            return companyId;
        }

        // Se nao encontrou CompanyId, lanca excecao de autorizacao
        // O middleware ExceptionHandlingMiddleware captura e retorna 403 Forbidden
        throw new UnauthorizedAccessException("Empresa nao identificada no token.");
    }

    // GET api/orders
    // Endpoint autenticado para listar pedidos da empresa autenticada
    // O parametro "role" na query string define o contexto da listagem:
    //   role=buyer → lista pedidos onde a empresa e COMPRADORA
    //   role=seller → lista pedidos onde a empresa e VENDEDORA
    // Suporta paginacao via query string (page, pageSize)
    // Retorna:
    //   200 OK com PaginatedResult<OrderDto>
    //   400 Bad Request se role for invalido
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<OrderDto>), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> GetAll(
        [FromQuery] string role = "buyer", // Papel: "buyer" (compradora) ou "seller" (vendedora)
        [FromQuery] int page = 1, // Numero da pagina (default 1)
        [FromQuery] int pageSize = 20, // Itens por pagina (default 20)
        CancellationToken cancellationToken = default)
    {
        // Obtem o CompanyId da empresa autenticada do JWT
        var companyId = GetCompanyId();

        // Determina se esta buscando como comprador ou vendedor
        // ToLowerInvariant normaliza para comparacao case-insensitive
        // asBuyer = true → busca pedidos onde companyId e o comprador
        // asBuyer = false → busca pedidos onde companyId e o vendedor
        bool asBuyer = role.ToLowerInvariant() switch
        {
            "buyer" => true,   // Empresa atuando como compradora
            "seller" => false, // Empresa atuando como vendedora
            _ => throw new BadHttpRequestException(
                "Role invalido. Use 'buyer' ou 'seller'.") // Role desconhecido → 400
        };

        // Chama o servico de pedido para listar com paginacao
        // GetByCompanyAsync retorna pedidos filtrados por empresa e papel
        var result = await _orderService.GetByCompanyAsync(
            companyId,
            asBuyer,
            page,
            pageSize,
            cancellationToken);

        // Ok() retorna 200 OK com o PaginatedResult no corpo JSON
        return Ok(result);
    }

    // GET api/orders/{id}
    // Endpoint autenticado para obter detalhes completos de um pedido
    // Inclui: itens do pedido, empresas compradora e vendedora, status, valores
    // Parametros:
    //   id: identificador unico do pedido (Guid)
    // Retorna:
    //   200 OK com OrderDto contendo todos os detalhes
    //   400 Bad Request se o pedido nao for encontrado
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta pedido nao encontrado
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Chama o servico de pedido para buscar detalhes completos pelo ID
        // GetByIdAsync carrega itens e propriedades de navegacao (empresas)
        var order = await _orderService.GetByIdAsync(id, cancellationToken);

        // Ok() retorna 200 OK com o OrderDto completo no corpo JSON
        return Ok(order);
    }

    // POST api/orders
    // Endpoint autenticado para criar um novo pedido de compra
    // O CompanyId do JWT e usado automaticamente como empresa compradora (buyer)
    // O fornecedor (sellerCompanyId) vem no corpo da requisicao
    // Fluxo:
    //   1. Extrai CompanyId do JWT (empresa compradora)
    //   2. Para cada item, busca o produto e obtem o preco base
    //   3. Cria entidade Order com buyer, seller e lista de OrderItems
    //   4. Persiste o pedido no banco
    // Restricoes de negocio:
    //   - Nao pode comprar de si mesmo (buyerCompanyId != sellerCompanyId)
    //   - O pedido deve ter pelo menos 1 item
    //   - Produtos inativos nao podem ser comprados
    // Retorna:
    //   201 Created com OrderDto
    //   400 Bad Request se dados invalidos ou produto nao encontrado
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)] // Documenta criacao
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto request, // Dados do pedido (itens e fornecedor)
        CancellationToken cancellationToken)
    {
        // Obtem o CompanyId do JWT — sera o comprador (buyer) do pedido
        // A empresa autenticada e automaticamente a compradora
        var companyId = GetCompanyId();

        // Chama o servico de pedido para criar o novo pedido
        // CreateAsync:
        //   1. Para cada item, busca produto e valida disponibilidade
        //   2. Cria OrderItems com preco base do produto
        //   3. Cria Order com buyer=companyId, seller=request.SellerCompanyId
        //   4. Persiste e retorna OrderDto
        var order = await _orderService.CreateAsync(companyId, request, cancellationToken);

        // CreatedAtAction retorna 201 Created com localizacao e corpo
        // nameof(GetById): referencia o endpoint GET por ID
        // routeValues: parametro id para URL → /api/orders/{id}
        // value: DTO do pedido criado no corpo da resposta
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PATCH api/orders/{id}/status
    // Endpoint autenticado para atualizar o status de um pedido
    // Apenas a empresa VENDEDORA (seller) pode alterar o status do pedido
    // O comprador pode apenas criar e visualizar pedidos
    // Usa PATCH (e nao PUT) porque e uma atualizacao PARCIAL (apenas o status)
    // Transicoes de status validas:
    //   - Confirm: Pendente → Confirmado (apenas pela vendedora)
    //   - Cancel: Pendente ou Confirmado → Cancelado (apenas pela vendedora)
    //   - Ship: Confirmado → Enviado (apenas pela vendedora)
    //   - Deliver: Enviado → Entregue (apenas pela vendedora)
    // A entidade Order valida as transicoes validas (nao permite pular etapas)
    // Retorna:
    //   200 OK com OrderDto atualizado
    //   400 Bad Request se pedido nao encontrado, status invalido ou sem permissao
    //   403 Forbidden se CompanyId nao estiver no token
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)] // Documenta sucesso
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documenta erro de negocio
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documenta sem empresa
    public async Task<IActionResult> UpdateStatus(
        Guid id, // ID do pedido a ter o status alterado (da URL)
        [FromBody] UpdateOrderStatusDto request, // Novo status do pedido (do corpo JSON)
        CancellationToken cancellationToken)
    {
        // Obtem o CompanyId do JWT — sera verificado contra o SellerCompanyId do pedido
        // Apenas a empresa vendedora pode alterar o status
        var companyId = GetCompanyId();

        // Chama o servico de pedido para atualizar o status
        // UpdateStatusAsync:
        //   1. Busca o pedido pelo ID
        //   2. Verifica se companyId == order.SellerCompanyId
        //   3. Executa a transicao de status (switch no status recebido)
        //   4. Persiste a alteracao e retorna OrderDto atualizado
        var order = await _orderService.UpdateStatusAsync(
            id,
            request.Status,
            companyId,
            cancellationToken);

        // Ok() retorna 200 OK com o OrderDto atualizado no corpo JSON
        return Ok(order);
    }
}
