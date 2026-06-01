// Importa as entidades de dominio que serao testadas
using EcommerceB2B.Domain.Entities;

// Importa os tipos enumerados do dominio (CompanyType)
using EcommerceB2B.Domain.Enums;

// Importa a excecao customizada que esperamos capturar nos testes de validacao
using EcommerceB2B.Domain.Exceptions;

// Namespace que agrupa os testes unitarios do dominio
namespace EcommerceB2B.Domain.Tests;

// Classe de testes para a entidade Company
// Testa construtor, metodos de validacao e regras de negocio
// xUnit cria uma nova instancia desta classe para cada [Fact] executado
// Isso garante isolamento entre testes (sem compartilhamento de estado)
public class CompanyTests
{
    // ─────────────────────────────────────────────────────────
    // Testes do Construtor
    // ─────────────────────────────────────────────────────────

    // Verifica que o construtor cria uma empresa com todos os campos preenchidos corretamente
    // Este e o cenario feliz (happy path): todos os dados sao validos
    [Fact] // Atributo xUnit: marca este metodo como um caso de teste unitario
    public void Constructor_ComDadosValidos_DeveCriarEmpresa()
    {
        // Arrange: prepara os dados de entrada para o teste
        var name = "Empresa ABC Ltda";
        var document = "12345678000199"; // CNPJ sem mascara (14 digitos)
        var type = CompanyType.Vendedor; // Tipo de empresa: Vendedor

        // Act: executa a acao que esta sendo testada (criacao da empresa)
        var company = new Company(name, document, type);

        // Assert: verifica se o resultado corresponde ao esperado
        // Cada Assert verifica uma propriedade da empresa criada
        Assert.Equal(name, company.Name);        // Nome deve ser igual ao informado
        Assert.Equal(document, company.Document); // Documento deve ser igual ao informado
        Assert.Equal(type, company.Type);         // Tipo deve ser o enum Vendedor
        Assert.True(company.IsActive);             // Empresa nova deve estar ativa
        Assert.NotEqual(Guid.Empty, company.Id);   // ID deve ser um GUID valido (nao vazio)
        Assert.True(company.CreatedAt > DateTime.MinValue); // Data de criacao deve ser preenchida
    }

    // Verifica que o tipo padrao da empresa e "Ambos" quando nao especificado
    // Empresas tipo Ambos podem comprar E vender na plataforma
    [Fact]
    public void Constructor_SemTipoEspecifico_DeveCriarComoAmbos()
    {
        // Arrange: dados basicos sem especificar o tipo da empresa
        var name = "Empresa Flex Ltda";
        var document = "98765432000199";

        // Act: cria a empresa SEM passar o parametro type
        // O construtor usa o valor padrao CompanyType.Ambos
        var company = new Company(name, document);

        // Assert: verifica que o tipo padrao e Ambos
        Assert.Equal(CompanyType.Ambos, company.Type);
    }

    // ─────────────────────────────────────────────────────────
    // Testes de Validacao do Construtor
    // ─────────────────────────────────────────────────────────

    // Verifica que construtor lanca DomainException quando o nome e vazio
    // Nome e obrigatorio: sem ele, a empresa nao pode ser identificada na plataforma
    [Fact]
    public void Constructor_ComNomeVazio_DeveLancarDomainException()
    {
        // Arrange: nome invalido (vazio)
        var document = "12345678000199";

        // Act e Assert: verifica que o construtor lanca DomainException
        // Assert.Throws<T> executa o codigo dentro da lambda e verifica se a excecao T e lancada
        // Se a excecao NAO for lancada, o teste FALHA
        var exception = Assert.Throws<DomainException>(() =>
            new Company("", document)); // Nome vazio deve lancar excecao

        // Verifica que a mensagem de erro contem a palavra "nome"
        // Isso garante que a excecao certa foi lancada (nao outra validacao)
        Assert.Contains("nome", exception.Message.ToLower());
    }

    // Verifica que construtor lanca DomainException quando o nome e apenas espacos
    // Espacos em branco nao sao um nome valido
    [Fact]
    public void Constructor_ComNomeApenasEspacos_DeveLancarDomainException()
    {
        // Arrange: nome com apenas espacos em branco (invalido)
        var document = "12345678000199";

        // Act e Assert: verifica que lanca DomainException
        var exception = Assert.Throws<DomainException>(() =>
            new Company("   ", document)); // Apenas espacos — deve lancar excecao

        // A mensagem deve mencionar que o nome e obrigatorio
        Assert.Contains("nome", exception.Message.ToLower());
    }

    // Verifica que construtor lanca DomainException quando o documento e vazio
    // CNPJ e obrigatorio para identificacao fiscal da empresa
    [Fact]
    public void Constructor_ComDocumentoVazio_DeveLancarDomainException()
    {
        // Arrange: documento invalido (vazio)
        var name = "Empresa ABC Ltda";

        // Act e Assert: verifica que lanca DomainException
        var exception = Assert.Throws<DomainException>(() =>
            new Company(name, "")); // Documento vazio — deve lancar excecao

        // A mensagem deve mencionar que o documento e obrigatorio
        Assert.Contains("documento", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes do Metodo SetName
    // ─────────────────────────────────────────────────────────

    // Verifica que SetName atualiza o nome da empresa com um valor valido
    [Fact]
    public void SetName_ComNomeValido_DeveAtualizarNome()
    {
        // Arrange: cria empresa com nome inicial
        var company = new Company("Nome Antigo", "12345678000199");

        // Act: atualiza o nome para um novo valor valido
        var novoNome = "Nome Atualizado Ltda";
        company.SetName(novoNome);

        // Assert: verifica que o nome foi atualizado
        Assert.Equal(novoNome, company.Name);
    }

    // Verifica que SetName lanca DomainException com nome vazio
    [Fact]
    public void SetName_ComNomeVazio_DeveLancarDomainException()
    {
        // Arrange: cria empresa valida
        var company = new Company("Empresa ABC", "12345678000199");

        // Act e Assert: verifica que lanca DomainException ao tentar nome vazio
        var exception = Assert.Throws<DomainException>(() =>
            company.SetName("")); // Nome vazio nao e permitido

        Assert.Contains("nome", exception.Message.ToLower());
    }

    // ─────────────────────────────────────────────────────────
    // Testes do Metodo SetType
    // ─────────────────────────────────────────────────────────

    // Verifica que SetType atualiza o tipo da empresa
    [Fact]
    public void SetType_ComTipoValido_DeveAtualizarTipo()
    {
        // Arrange: cria empresa como Ambos
        var company = new Company("Empresa ABC", "12345678000199");

        // Act: altera o tipo para Comprador
        company.SetType(CompanyType.Comprador);

        // Assert: o tipo deve ser Comprador
        Assert.Equal(CompanyType.Comprador, company.Type);
    }

    // ─────────────────────────────────────────────────────────
    // Testes dos Metodos Activate e Deactivate
    // ─────────────────────────────────────────────────────────

    // Verifica que Deactivate desativa a empresa
    [Fact]
    public void Deactivate_DeveDefinirIsActiveComoFalso()
    {
        // Arrange: cria empresa ativa
        var company = new Company("Empresa ABC", "12345678000199");

        // Act: desativa a empresa
        company.Deactivate();

        // Assert: IsActive deve ser false
        Assert.False(company.IsActive);
    }

    // Verifica que Activate reativa uma empresa desativada
    [Fact]
    public void Activate_AposDeactivate_DeveReativarEmpresa()
    {
        // Arrange: cria empresa e a desativa
        var company = new Company("Empresa ABC", "12345678000199");
        company.Deactivate(); // IsActive = false

        // Act: reativa a empresa
        company.Activate();

        // Assert: IsActive deve voltar a ser true
        Assert.True(company.IsActive);
    }
}
