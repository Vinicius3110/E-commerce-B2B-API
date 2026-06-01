// Importa as entidades de dominio que serao testadas
using EcommerceB2B.Domain.Entities;

// Importa os tipos enumerados do dominio (OrderStatus)
using EcommerceB2B.Domain.Enums;

// Importa a excecao customizada que esperamos capturar nos testes de validacao
using EcommerceB2B.Domain.Exceptions;

// Namespace que agrupa os testes unitarios do dominio
namespace EcommerceB2B.Domain.Tests;

// Classe de testes para a entidade Order (pedido) e sua maquina de estados
// Testa criacao, calculo de total e transicoes validas/invalidas de status
// A maquina de estados do pedido controla o ciclo de vida completo:
//   Pendente → Confirmado → Enviado → Entregue
//            ↘ Cancelado ↗
public class OrderTests
{
    // Dados reutilizaveis nos testes: IDs de empresas e produtos
    // readonly: valores constantes apos inicializacao, evitam duplicacao
    private readonly Guid _buyerId = Guid.NewGuid();  // Empresa compradora
    private readonly Guid _sellerId = Guid.NewGuid();  // Empresa vendedora
    private readonly Guid _productId = Guid.NewGuid(); // Produto de exemplo

    // ─────────────────────────────────────────────────────────
    // Helper: cria um OrderItem valido para uso nos testes
    // ─────────────────────────────────────────────────────────

    // Metodo auxiliar que cria um item de pedido valido com dados padrao
    // Centraliza a criacao para evitar duplicacao de codigo nos testes
    // Parametros com valores padrao permitem customizacao em testes especificos
    private OrderItem CriarItemValido(
        int quantity = 2,
        decimal unitPrice = 100m) // m sufixo indica literal decimal (precisao financeira)
    {
        // Cria e retorna um OrderItem com o produto padrao, quantidade e preco
        return new OrderItem(_productId, quantity, unitPrice);
    }

    // ─────────────────────────────────────────────────────────
    // Testes do Construtor do Pedido (Order)
    // ─────────────────────────────────────────────────────────

    // Verifica que o construtor cria um pedido valido com itens e status correto
    // Cenario feliz (happy path): todos os dados sao validos
    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarPedido()
    {
        // Arrange: cria itens validos para o pedido
        var items = new List<OrderItem>
        {
            CriarItemValido(quantity: 2, unitPrice: 150m),
            CriarItemValido(quantity: 3, unitPrice: 100m)
        };

        // Act: cria o pedido com comprador, vendedor e itens
        var order = new Order(_buyerId, _sellerId, items);

        // Assert: verifica que todos os campos foram preenchidos corretamente
        Assert.Equal(_buyerId, order.BuyerCompanyId);  // Comprador correto
        Assert.Equal(_sellerId, order.SellerCompanyId); // Vendedor correto
        Assert.Equal(OrderStatus.Pendente, order.Status); // Status inicial: Pendente
        Assert.NotEqual(Guid.Empty, order.Id);           // ID unico gerado
        Assert.Equal(2, order.Items.Count);              // Dois itens no pedido

        // TotalAmount = (2 * 150) + (3 * 100) = 300 + 300 = 600
        Assert.Equal(600m, order.TotalAmount);
    }

    // Verifica que o pedido lanca DomainException quando a lista de itens e nula
    [Fact]
    public void Constructor_ComItensNulo_DeveLancarDomainException()
    {
        // Act e Assert: verifica que lanca DomainException ao passar null como itens
        var exception = Assert.Throws<DomainException>(() =>
            new Order(_buyerId, _sellerId, null!)); // null! suprime o warning de nulabilidade

        // A mensagem deve mencionar que e necessario pelo menos um item
        Assert.Contains("item", exception.Message.ToLower());
    }

    // Verifica que o pedido lanca DomainException quando a lista de itens esta vazia
    [Fact]
    public void Constructor_ComItensVazio_DeveLancarDomainException()
    {
        // Arrange: lista vazia de itens
        var items = new List<OrderItem>();

        // Act e Assert: verifica que lanca DomainException
        var exception = Assert.Throws<DomainException>(() =>
            new Order(_buyerId, _sellerId, items));

        Assert.Contains("item", exception.Message.ToLower());
    }

    // Verifica que nao e possivel criar pedido comprando de si mesmo
    // Regra de negocio fundamental do B2B: empresas diferentes
    [Fact]
    public void Constructor_CompradorIgualVendedor_DeveLancarDomainException()
    {
        // Arrange: mesmo ID para comprador e vendedor
        var mesmoId = Guid.NewGuid();
        var items = new List<OrderItem> { CriarItemValido() };

        // Act e Assert: verifica que lanca DomainException
        var exception = Assert.Throws<DomainException>(() =>
            new Order(mesmoId, mesmoId, items)); // Comprador == Vendedor

        // A mensagem deve mencionar que auto-compra nao e permitida
        Assert.Contains("si mesma", exception.Message.ToLower());
    }

    // Verifica que o comprador nao pode ser Guid.Empty
    [Fact]
    public void Constructor_ComBuyerIdVazio_DeveLancarDomainException()
    {
        // Arrange: itens validos, mas buyerId = Guid.Empty
        var items = new List<OrderItem> { CriarItemValido() };

        // Act e Assert: verifica que lanca DomainException
        var exception = Assert.Throws<DomainException>(() =>
            new Order(Guid.Empty, _sellerId, items));

        Assert.Contains("compradora", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes de Transicao de Status — Fluxo Feliz (Happy Path)
    // ─────────────────────────────────────────────────────────

    // Verifica a transicao: Pendente → Confirmado (via metodo Confirm)
    // Apenas o vendedor pode confirmar um pedido pendente
    [Fact]
    public void Confirm_DePendenteParaConfirmado_DeveAlterarStatus()
    {
        // Arrange: cria pedido no status inicial (Pendente)
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });

        // Act: confirma o pedido
        order.Confirm();

        // Assert: status deve ser Confirmado
        Assert.Equal(OrderStatus.Confirmado, order.Status);
    }

    // Verifica a transicao: Pendente → Cancelado (via metodo Cancel)
    // Comprador pode cancelar antes da confirmacao do vendedor
    [Fact]
    public void Cancel_DePendenteParaCancelado_DeveAlterarStatus()
    {
        // Arrange: cria pedido pendente
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });

        // Act: cancela o pedido (ainda pendente, permitido)
        order.Cancel();

        // Assert: status deve ser Cancelado
        Assert.Equal(OrderStatus.Cancelado, order.Status);
    }

    // Verifica a transicao: Confirmado → Cancelado (via metodo Cancel)
    // Pode ser cancelado apos confirmacao, por acordo entre as partes
    [Fact]
    public void Cancel_DeConfirmadoParaCancelado_DeveAlterarStatus()
    {
        // Arrange: cria e confirma o pedido primeiro
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm(); // Pendente → Confirmado

        // Act: cancela o pedido confirmado
        order.Cancel();

        // Assert: status deve ser Cancelado
        Assert.Equal(OrderStatus.Cancelado, order.Status);
    }

    // Verifica a transicao: Confirmado → Enviado (via metodo Ship)
    // Apenas apos confirmar, o vendedor pode despachar
    [Fact]
    public void Ship_DeConfirmadoParaEnviado_DeveAlterarStatus()
    {
        // Arrange: cria e confirma o pedido
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm(); // Pendente → Confirmado

        // Act: despacha/envia o pedido
        order.Ship();

        // Assert: status deve ser Enviado
        Assert.Equal(OrderStatus.Enviado, order.Status);
    }

    // Verifica a transicao: Enviado → Entregue (via metodo Deliver)
    // Status final do ciclo de vida do pedido
    [Fact]
    public void Deliver_DeEnviadoParaEntregue_DeveAlterarStatus()
    {
        // Arrange: cria, confirma e envia o pedido
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm(); // Pendente → Confirmado
        order.Ship();     // Confirmado → Enviado

        // Act: entrega o pedido
        order.Deliver();

        // Assert: status deve ser Entregue (status final)
        Assert.Equal(OrderStatus.Entregue, order.Status);
    }

    // ─────────────────────────────────────────────────────────
    // Testes de Transicoes INVALIDAS
    // ─────────────────────────────────────────────────────────

    // Verifica que nao pode confirmar (Confirm) um pedido que nao esta Pendente
    // A confirmacao so faz sentido no inicio do fluxo
    [Fact]
    public void Confirm_DeStatusDiferenteDePendente_DeveLancarDomainException()
    {
        // Arrange: cria e confirma o pedido (ja esta Confirmado)
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm(); // Agora esta Confirmado, nao Pendente

        // Act e Assert: tentar confirmar novamente deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Confirm());
        Assert.Contains("Pendente", exception.Message); // Mensagem deve mencionar Pendente
    }

    // Verifica que nao pode despachar (Ship) um pedido Pendente (sem confirmar antes)
    // Deve seguir a ordem: Pendente → Confirmado → Enviado (nao pular etapas)
    [Fact]
    public void Ship_DePendente_DeveLancarDomainException()
    {
        // Arrange: cria pedido pendente (nao confirmado)
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });

        // Act e Assert: tentar enviar sem confirmar deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Ship());
        Assert.Contains("Confirmado", exception.Message); // Mensagem deve mencionar Confirmado
    }

    // Verifica que nao pode entregar (Deliver) um pedido Pendente (sem confirmar e enviar)
    // Nao pode pular etapas no fluxo do pedido
    [Fact]
    public void Deliver_DePendente_DeveLancarDomainException()
    {
        // Arrange: cria pedido pendente
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });

        // Act e Assert: tentar entregar diretamente deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Deliver());
        Assert.Contains("Enviado", exception.Message); // Mensagem deve mencionar Enviado
    }

    // Verifica que nao pode cancelar um pedido que ja foi enviado
    // Apos o envio, o pedido ja saiu do controle administrativo
    [Fact]
    public void Cancel_DeEnviado_DeveLancarDomainException()
    {
        // Arrange: cria, confirma e envia o pedido
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm();
        order.Ship(); // Agora esta Enviado

        // Act e Assert: tentar cancelar apos envio deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Contains("Pendente", exception.Message); // Mensagem lista status validos
    }

    // Verifica que nao pode cancelar um pedido ja entregue (status final)
    [Fact]
    public void Cancel_DeEntregue_DeveLancarDomainException()
    {
        // Arrange: cria, confirma, envia e entrega o pedido
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm();
        order.Ship();
        order.Deliver(); // Agora esta Entregue (status final)

        // Act e Assert: tentar cancelar pedido ja entregue deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Contains("Pendente", exception.Message);
    }

    // Verifica que nao pode entregar um pedido ja entregue
    [Fact]
    public void Deliver_DeEntregue_DeveLancarDomainException()
    {
        // Arrange: pedido no status final (Entregue)
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        order.Confirm();
        order.Ship();
        order.Deliver(); // Ja esta Entregue

        // Act e Assert: entregar novamente deve lancar excecao
        var exception = Assert.Throws<DomainException>(() => order.Deliver());
        Assert.Contains("Enviado", exception.Message);
    }

    // ─────────────────────────────────────────────────────────
    // Testes do OrderItem (Preco Total)
    // ─────────────────────────────────────────────────────────

    // Verifica que o OrderItem calcula TotalPrice corretamente
    // TotalPrice = Quantity * UnitPrice (calculado no construtor)
    [Fact]
    public void OrderItem_DeveCalcularPrecoTotalCorretamente()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 5;
        var unitPrice = 49.90m;

        // Act: cria o item do pedido
        var item = new OrderItem(productId, quantity, unitPrice);

        // Assert: verifica o calculo do preco total
        // 5 * 49.90 = 249.50
        Assert.Equal(249.50m, item.TotalPrice);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
    }

    // Verifica que OrderItem rejeita quantidade zero
    [Fact]
    public void OrderItem_ComQuantidadeZero_DeveLancarDomainException()
    {
        // Act e Assert: quantidade 0 nao e permitida
        var exception = Assert.Throws<DomainException>(() =>
            new OrderItem(Guid.NewGuid(), 0, 100m));
        Assert.Contains("quantidade", exception.Message.ToLower());
    }

    // Verifica que OrderItem rejeita quantidade negativa
    [Fact]
    public void OrderItem_ComQuantidadeNegativa_DeveLancarDomainException()
    {
        // Act e Assert: quantidade negativa nao faz sentido
        var exception = Assert.Throws<DomainException>(() =>
            new OrderItem(Guid.NewGuid(), -1, 100m));
        Assert.Contains("quantidade", exception.Message.ToLower());
    }

    // Verifica que OrderItem rejeita preco unitario zero
    // Em B2B, produtos nao tem preco zero (diferente de B2C com brindes)
    [Fact]
    public void OrderItem_ComPrecoZero_DeveLancarDomainException()
    {
        // Act e Assert: preco zero nao e permitido (> 0 exigido)
        var exception = Assert.Throws<DomainException>(() =>
            new OrderItem(Guid.NewGuid(), 1, 0m));
        Assert.Contains("preço", exception.Message.ToLower());
    }

    // Verifica que OrderItem rejeita preco unitario negativo
    [Fact]
    public void OrderItem_ComPrecoNegativo_DeveLancarDomainException()
    {
        // Act e Assert: preco negativo nao faz sentido
        var exception = Assert.Throws<DomainException>(() =>
            new OrderItem(Guid.NewGuid(), 1, -50m));
        Assert.Contains("preço", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes de UpdatedAt (marca de tempo de atualizacao)
    // ─────────────────────────────────────────────────────────

    // Verifica que UpdatedAt e atualizado apos transicao de status
    [Fact]
    public void Confirm_DeveAtualizarUpdatedAt()
    {
        // Arrange: cria pedido e captura o UpdatedAt inicial
        var order = new Order(_buyerId, _sellerId, new List<OrderItem> { CriarItemValido() });
        var updatedAtInicial = order.UpdatedAt;

        // Pequena pausa para garantir que o tempo vai diferir
        // Em maquinas rapidas, a transicao pode ser mais rapida que a resolucao do DateTime
        Thread.Sleep(10);

        // Act: confirma o pedido
        order.Confirm();

        // Assert: UpdatedAt deve ser posterior ao valor inicial
        Assert.True(order.UpdatedAt > updatedAtInicial,
            "UpdatedAt deve ser atualizado apos transicao de status");
    }
}
