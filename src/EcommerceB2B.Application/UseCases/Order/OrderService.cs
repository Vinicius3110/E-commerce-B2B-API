// Importa o DTO de paginacao generica compartilhada
using EcommerceB2B.Application.DTOs.Common;

// Importa os DTOs de pedido definidos na camada de aplicacao
using EcommerceB2B.Application.DTOs.Order;

// Importa as entidades do dominio (Order, OrderItem)
using EcommerceB2B.Domain.Entities;

// Alias para evitar conflito entre os nomes das entidades Order/OrderItem e o namespace Order
// O compilador confundiria Order (entidade) com Order (namespace deste arquivo)
using OrderEntity = EcommerceB2B.Domain.Entities.Order;
using OrderItemEntity = EcommerceB2B.Domain.Entities.OrderItem;

// Importa os tipos enumerados do dominio (OrderStatus)
using EcommerceB2B.Domain.Enums;

// Importa a excecao customizada de dominio para erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Importa as interfaces de repositorio definidas no dominio
using EcommerceB2B.Domain.Interfaces;

// Namespace que agrupa os servicos de pedido na camada de aplicacao
namespace EcommerceB2B.Application.UseCases.Order;

// Servico de pedido: orquestra operacoes relacionadas a pedidos de compra B2B
// Gerencia criacao, consulta, listagem e transicoes de status do pedido
// Depende dos repositorios de pedido (IOrderRepository) e produto (IProductRepository)
public class OrderService
{
    // Repositorio de pedidos (definido no dominio, implementado no Infrastructure)
    // Fornece acesso a dados de pedidos, itens e metadados de compra
    private readonly IOrderRepository _orderRepository;

    // Repositorio de produtos (definido no dominio, implementado no Infrastructure)
    // Usado para buscar precos e validar produtos ao criar itens do pedido
    private readonly IProductRepository _productRepository;

    // Construtor que recebe os repositorios por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta as implementacoes concretas
    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        // Armazena as referencias dos repositorios
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // Caso de uso: Listar pedidos de uma empresa (como compradora ou vendedora)
    // O parametro asBuyer controla se busca pedidos de compra ou de venda
    // Retorna resultado paginado com metadados de navegacao
    public async Task<PaginatedResult<OrderDto>> GetByCompanyAsync(
        Guid companyId,
        bool asBuyer,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Chama o repositorio para buscar os pedidos com paginacao
        // asBuyer: true → pedidos onde a empresa e compradora
        // asBuyer: false → pedidos onde a empresa e vendedora
        var (items, totalCount) = await _orderRepository.GetByCompanyAsync(
            companyId,
            asBuyer,
            page,
            pageSize,
            cancellationToken);

        // Converte a lista de entidades Order para lista de OrderDto
        var orderDtos = items.Select(MapToDto).ToList();

        // Retorna o resultado paginado com metadados
        return new PaginatedResult<OrderDto>
        {
            Items = orderDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Caso de uso: Obter pedido por ID com detalhes completos
    // Inclui itens do pedido e empresas compradora e vendedora
    // Se o pedido nao existir, lanca DomainException
    public async Task<OrderDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Busca o pedido no repositorio com todos os detalhes
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        // Se o pedido nao for encontrado, lanca excecao
        if (order is null)
        {
            throw new DomainException("Pedido não encontrado.");
        }

        // Converte a entidade para DTO e retorna
        return MapToDto(order);
    }

    // Caso de uso: Criar um novo pedido de compra
    // Fluxo completo:
    //   1. Para cada item, buscar o produto no repositorio para obter o preco base
    //   2. Criar OrderItem com productId, quantity e unitPrice (base do produto)
    //   3. Criar Order com buyerId, sellerId e a lista de itens
    //   4. Persistir o pedido no banco
    // Retorna o DTO do pedido criado
    public async Task<OrderDto> CreateAsync(
        Guid buyerCompanyId,
        CreateOrderDto request,
        CancellationToken cancellationToken = default)
    {
        // Lista que acumulara os OrderItems criados a partir dos itens do DTO
        var orderItems = new List<OrderItemEntity>();

        // 1. Para cada item no DTO de criacao do pedido
        foreach (var itemDto in request.Items)
        {
            // Busca o produto no repositorio para obter o preco base atual
            // O preco base do produto e usado como preco unitario do item
            var product = await _productRepository.GetByIdAsync(
                itemDto.ProductId,
                cancellationToken: cancellationToken);

            // Se o produto nao existir, lanca excecao
            if (product is null)
            {
                throw new DomainException(
                    $"Produto com ID '{itemDto.ProductId}' não encontrado.");
            }

            // Verifica se o produto esta ativo (so e possivel comprar produtos ativos)
            if (!product.IsActive)
            {
                throw new DomainException(
                    $"O produto '{product.Name}' não está disponível para compra.");
            }

            // 2. Cria o OrderItem com os dados do DTO e o preco do produto
            // O construtor de OrderItem valida:
            //   - productId nao pode ser Guid.Empty
            //   - quantity deve ser pelo menos 1
            //   - unitPrice deve ser maior que zero
            var orderItem = new OrderItemEntity(
                itemDto.ProductId,
                itemDto.Quantity,
                product.BasePrice); // Usa o preco base do produto como preco unitario

            // Adiciona o item a lista de itens do pedido
            orderItems.Add(orderItem);
        }

        // 3. Cria a entidade Order com os dados do DTO e a lista de itens
        // O construtor de Order valida:
        //   - buyerCompanyId e sellerCompanyId nao podem ser Guid.Empty
        //   - buyerCompanyId != sellerCompanyId (nao pode comprar de si mesmo)
        //   - A lista de itens nao pode ser nula ou vazia
        var order = new OrderEntity(
            buyerCompanyId,
            request.SellerCompanyId,
            orderItems);

        // 4. Persiste o pedido no banco via repositorio
        await _orderRepository.AddAsync(order, cancellationToken);

        // Converte a entidade criada para DTO e retorna
        return MapToDto(order);
    }

    // Caso de uso: Atualizar o status de um pedido
    // Fluxo:
    //   1. Buscar o pedido
    //   2. Verificar que a empresa que atualiza e a vendedora (apenas vendedor muda status)
    //   3. Fazer switch no status recebido e chamar o metodo correspondente da entidade
    //   4. Persistir a alteracao
    // Os metodos da entidade validam as transicoes de status validas
    public async Task<OrderDto> UpdateStatusAsync(
        Guid orderId,
        string status,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // 1. Busca o pedido pelo ID no repositorio
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        // Se o pedido nao existir, lanca excecao
        if (order is null)
        {
            throw new DomainException("Pedido não encontrado.");
        }

        // 2. Verifica ownership: apenas a empresa vendedora pode alterar o status do pedido
        // O comprador pode apenas criar e visualizar pedidos
        if (order.SellerCompanyId != companyId)
        {
            throw new DomainException(
                "Apenas a empresa vendedora pode alterar o status do pedido.");
        }

        // 3. Executa a transicao de status baseada na string recebida
        // Converte para minusculo (ToLowerInvariant) para comparacao case-insensitive
        // Cada metodo da entidade valida se a transicao e permitida:
        //   - Confirm: Pendente → Confirmado
        //   - Cancel: Pendente ou Confirmado → Cancelado
        //   - Ship: Confirmado → Enviado
        //   - Deliver: Enviado → Entregue
        switch (status.ToLowerInvariant())
        {
            case "confirm":
                // Confirma o pedido (apenas de Pendente)
                order.Confirm();
                break;

            case "cancel":
                // Cancela o pedido (de Pendente ou Confirmado)
                order.Cancel();
                break;

            case "ship":
                // Envia/despacha o pedido (apenas de Confirmado)
                order.Ship();
                break;

            case "deliver":
                // Marca o pedido como entregue (apenas de Enviado)
                order.Deliver();
                break;

            default:
                // Se a string nao corresponder a nenhum status valido, lanca excecao
                throw new DomainException(
                    "Status inválido. Valores válidos: Confirm, Cancel, Ship, Deliver.");
        }

        // 4. Persiste a alteracao de status no banco
        _orderRepository.Update(order);

        // Converte a entidade atualizada para DTO e retorna
        return MapToDto(order);
    }

    // Metodo auxiliar privado: converte entidade Order para OrderDto
    // Mapeia todos os campos, incluindo itens e nomes das empresas
    // E static pois nao depende de estado da instancia
    private static OrderDto MapToDto(OrderEntity order)
    {
        // Cria e retorna o DTO preenchido com os dados da entidade
        return new OrderDto
        {
            // Copia o identificador unico do pedido
            Id = order.Id,

            // Copia o ID da empresa compradora
            BuyerCompanyId = order.BuyerCompanyId,

            // Copia o nome da empresa compradora (da propriedade de navegacao)
            // Usa operador ?. para evitar NullReferenceException se BuyerCompany nao foi carregado
            BuyerCompanyName = order.BuyerCompany?.Name ?? string.Empty,

            // Copia o ID da empresa vendedora
            SellerCompanyId = order.SellerCompanyId,

            // Copia o nome da empresa vendedora (da propriedade de navegacao)
            SellerCompanyName = order.SellerCompany?.Name ?? string.Empty,

            // Converte o enum OrderStatus para string usando ToString()
            // Ex: OrderStatus.Pendente → "Pendente"
            Status = order.Status.ToString(),

            // Copia o valor total do pedido
            TotalAmount = order.TotalAmount,

            // Copia a data de criacao
            CreatedAt = order.CreatedAt,

            // Mapeia a colecao de itens do pedido para DTOs de item
            // Para cada OrderItem, cria um OrderItemDto com os dados do item e produto
            Items = order.Items.Select(item => new OrderItemDto
            {
                // Copia o ID do item
                Id = item.Id,

                // Copia o ID do produto comprado
                ProductId = item.ProductId,

                // Copia o nome do produto (da propriedade de navegacao)
                // Se Product nao foi carregado, fica string vazia
                ProductName = item.Product?.Name ?? string.Empty,

                // Copia a quantidade comprada
                Quantity = item.Quantity,

                // Copia o preco unitario no momento da compra
                UnitPrice = item.UnitPrice,

                // Copia o preco total do item (quantidade * preco unitario)
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
}
