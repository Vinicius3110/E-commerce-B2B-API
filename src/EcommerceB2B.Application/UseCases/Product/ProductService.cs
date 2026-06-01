// Importa o DTO de paginacao generica compartilhada
using EcommerceB2B.Application.DTOs.Common;

// Importa os DTOs de produto definidos na camada de aplicacao
using EcommerceB2B.Application.DTOs.Product;

// Importa as entidades do dominio (Product, ProductPrice)
using EcommerceB2B.Domain.Entities;

// Alias para evitar conflito entre os nomes das entidades Product/ProductPrice e o namespace Product
// O compilador confundiria Product (entidade) com Product (namespace deste arquivo)
using ProductEntity = EcommerceB2B.Domain.Entities.Product;
using ProductPriceEntity = EcommerceB2B.Domain.Entities.ProductPrice;

// Importa a excecao customizada de dominio para erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Importa a interface de repositorio de produtos definida no dominio
using EcommerceB2B.Domain.Interfaces;

// Namespace que agrupa os servicos de produto na camada de aplicacao
namespace EcommerceB2B.Application.UseCases.Product;

// Servico de produto: orquestra operacoes de CRUD relacionadas a produtos do marketplace B2B
// Gerencia listagem, criacao, atualizacao, desativacao e precos customizados
// Depende do repositorio de produtos (IProductRepository)
public class ProductService
{
    // Repositorio de produtos (definido no dominio, implementado no Infrastructure)
    // Fornece acesso a dados de produtos, precos personalizados e verificacao de SKU
    private readonly IProductRepository _productRepository;

    // Construtor que recebe o repositorio por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta a implementacao concreta
    public ProductService(IProductRepository productRepository)
    {
        // Armazena a referencia do repositorio
        _productRepository = productRepository;
    }

    // Caso de uso: Listar produtos com filtros e paginacao
    // Permite filtrar por categoria, vendedor, faixa de preco
    // Retorna resultado paginado com metadados de navegacao
    public async Task<PaginatedResult<ProductDto>> GetFilteredAsync(
        Guid? categoryId = null,
        Guid? sellerCompanyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Chama o repositorio com todos os filtros
        // O repositorio aplica cada filtro se o valor nao for nulo
        var (items, totalCount) = await _productRepository.GetFilteredAsync(
            categoryId,
            sellerCompanyId,
            minPrice,
            maxPrice,
            page,
            pageSize,
            cancellationToken);

        // Converte a lista de entidades para lista de DTOs
        var productDtos = items.Select(MapToDto).ToList();

        // Retorna o resultado paginado com metadados
        return new PaginatedResult<ProductDto>
        {
            Items = productDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Caso de uso: Obter produto por ID com detalhes completos
    // Inclui empresa vendedora e categoria (propriedades de navegacao)
    // Se o produto nao existir, lanca DomainException
    public async Task<ProductDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Busca o produto no repositorio com includeDetails: true
        // Isso carrega as propriedades de navegacao Company e Category
        var product = await _productRepository.GetByIdAsync(
            id,
            includeDetails: true,
            cancellationToken: cancellationToken);

        // Se o produto nao for encontrado, lanca excecao
        if (product is null)
        {
            throw new DomainException("Produto não encontrado.");
        }

        // Converte a entidade para DTO e retorna
        return MapToDto(product);
    }

    // Caso de uso: Criar um novo produto no catalogo da empresa vendedora
    // Fluxo:
    //   1. Verificar se o SKU ja existe para esta empresa
    //   2. Criar a entidade Product
    //   3. Persistir no banco
    // Retorna o DTO do produto criado
    public async Task<ProductDto> CreateAsync(
        Guid companyId,
        CreateProductDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Verifica se ja existe um produto com o mesmo SKU para esta empresa
        // SkuExistsAsync retorna true se o SKU ja estiver em uso
        // SKU deve ser unico por empresa vendedora (uma empresa nao pode ter dois produtos com mesmo SKU)
        var skuExists = await _productRepository.SkuExistsAsync(
            companyId,
            request.Sku,
            excludeProductId: null, // Nao exclui nenhum produto (criacao, nao atualizacao)
            cancellationToken: cancellationToken);

        // Se o SKU ja existir, lanca excecao de dominio
        if (skuExists)
        {
            throw new DomainException(
                $"Já existe um produto com o SKU '{request.Sku}' para esta empresa.");
        }

        // 2. Cria a entidade Product usando o construtor publico
        // O construtor valida todos os campos obrigatorios:
        //   - companyId e categoryId nao podem ser Guid.Empty
        //   - nome e SKU nao podem ser vazios
        //   - basePrice nao pode ser negativo
        //   - stockQuantity nao pode ser negativo
        var product = new ProductEntity(
            companyId,
            request.CategoryId,
            request.Name,
            request.Sku,
            request.BasePrice,
            request.StockQuantity,
            request.Description);

        // 3. Persiste o produto no banco via repositorio
        await _productRepository.AddAsync(product, cancellationToken);

        // Converte a entidade criada para DTO e retorna
        return MapToDto(product);
    }

    // Caso de uso: Atualizar um produto existente
    // Fluxo:
    //   1. Buscar produto e verificar existencia
    //   2. Verificar ownership (apenas o dono pode editar)
    //   3. Verificar unicidade do SKU (excluindo o proprio produto)
    //   4. Aplicar alteracoes e persistir
    public async Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductDto request,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // 1. Busca o produto pelo ID (sem detalhes por performance)
        var product = await _productRepository.GetByIdAsync(id, cancellationToken: cancellationToken);

        // Se o produto nao existir, lanca excecao
        if (product is null)
        {
            throw new DomainException("Produto não encontrado.");
        }

        // 2. Verifica se a empresa que esta tentando editar e a dona do produto
        // Apenas a empresa vendedora pode alterar seus proprios produtos
        if (product.CompanyId != companyId)
        {
            // Lanca excecao de autorizacao — empresa nao e dona do produto
            throw new DomainException(
                "Você não tem permissão para editar este produto.");
        }

        // 3. Verifica se o novo SKU ja existe para esta empresa (excluindo o produto atual)
        // excludeProductId: id → ignora o produto atual na verificacao
        // Isso permite manter o mesmo SKU sem acusar duplicidade
        var skuExists = await _productRepository.SkuExistsAsync(
            companyId,
            request.Sku,
            excludeProductId: id,
            cancellationToken: cancellationToken);

        // Se o SKU ja estiver em uso por outro produto, lanca excecao
        if (skuExists)
        {
            throw new DomainException(
                $"Já existe outro produto com o SKU '{request.Sku}' para esta empresa.");
        }

        // 4. Aplica as alteracoes usando os metodos da entidade (rich domain model)
        // Cada SetXxx valida os dados antes de atribuir
        product.SetName(request.Name);
        product.SetDescription(request.Description);
        product.SetSku(request.Sku);
        product.SetBasePrice(request.BasePrice);
        product.SetStockQuantity(request.StockQuantity);

        // Persiste as alteracoes no banco
        _productRepository.Update(product);

        // Converte a entidade atualizada para DTO e retorna
        return MapToDto(product);
    }

    // Caso de uso: Desativar um produto (soft delete)
    // Fluxo:
    //   1. Buscar produto
    //   2. Verificar ownership (apenas o dono pode desativar)
    //   3. Chamar Deactivate() e persistir
    public async Task DeactivateAsync(
        Guid id,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // 1. Busca o produto pelo ID
        var product = await _productRepository.GetByIdAsync(id, cancellationToken: cancellationToken);

        // Se o produto nao existir, lanca excecao
        if (product is null)
        {
            throw new DomainException("Produto não encontrado.");
        }

        // 2. Verifica ownership: apenas a empresa dona do produto pode desativa-lo
        if (product.CompanyId != companyId)
        {
            throw new DomainException(
                "Você não tem permissão para desativar este produto.");
        }

        // 3. Desativa o produto (soft delete: IsActive = false)
        // O registro permanece no banco mas o produto nao aparece no catalogo
        product.Deactivate();

        // Persiste a desativacao
        _productRepository.Update(product);
    }

    // Caso de uso: Definir um preco customizado para um comprador especifico
    // Permite negociacao B2B: cada comprador pode ter um preco diferente
    // Vincula o preco a uma quantidade minima de compra
    public async Task SetCustomPriceAsync(
        Guid productId,
        CreateProductPriceDto request,
        CancellationToken cancellationToken = default)
    {
        // Verifica se o produto existe no banco
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken: cancellationToken);

        // Se o produto nao existir, lanca excecao
        if (product is null)
        {
            throw new DomainException("Produto não encontrado.");
        }

        // Cria a entidade ProductPrice com os dados do DTO
        // O construtor valida:
        //   - productId e companyId nao podem ser Guid.Empty
        //   - customPrice deve ser maior que zero
        //   - minQuantity deve ser pelo menos 1
        var productPrice = new ProductPriceEntity(
            productId,
            request.BuyerCompanyId,
            request.CustomPrice,
            request.MinQuantity);

        // Persiste o preco customizado no banco via repositorio
        await _productRepository.AddPriceAsync(productPrice, cancellationToken);
    }

    // Metodo auxiliar privado: converte entidade Product para ProductDto
    // Mapeia todos os campos, incluindo propriedades de navegacao (Company, Category)
    // E static pois nao depende de estado da instancia
    private static ProductDto MapToDto(ProductEntity product)
    {
        // Cria e retorna o DTO preenchido com os dados da entidade
        return new ProductDto
        {
            // Copia o identificador unico do produto
            Id = product.Id,

            // Copia o ID da empresa vendedora (chave estrangeira)
            CompanyId = product.CompanyId,

            // Copia o nome da empresa vendedora (da propriedade de navegacao)
            // Usa operador ?. para evitar NullReferenceException se Company nao foi carregado
            // ?? string.Empty garante que o valor nunca sera null
            CompanyName = product.Company?.Name ?? string.Empty,

            // Copia o ID da categoria (chave estrangeira)
            CategoryId = product.CategoryId,

            // Copia o nome da categoria (da propriedade de navegacao)
            CategoryName = product.Category?.Name ?? string.Empty,

            // Copia o nome do produto
            Name = product.Name,

            // Copia a descricao (pode ser nula)
            Description = product.Description,

            // Copia o SKU do produto
            Sku = product.Sku,

            // Copia o preco base
            BasePrice = product.BasePrice,

            // Copia a quantidade em estoque
            StockQuantity = product.StockQuantity,

            // Copia o status de atividade
            IsActive = product.IsActive,

            // Copia a data de criacao
            CreatedAt = product.CreatedAt
        };
    }
}
