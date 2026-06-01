// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// A classe ProductPrice representa um preço customizado para um cliente específico
// Permite negociação B2B: cada comprador pode ter um preço diferente para o mesmo produto
// O preço customizado é vinculado a uma quantidade mínima de compra
public class ProductPrice
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    private ProductPrice()
    {
    }

    // Construtor público: cria um preço customizado vinculando produto, empresa compradora,
    // valor do preço e quantidade mínima para aplicação do preço
    public ProductPrice(Guid productId, Guid companyId, decimal customPrice, int minQuantity)
    {
        // Valida que o ID do produto não é vazio
        if (productId == Guid.Empty)
        {
            // Lança exceção informando que o produto é obrigatório
            throw new DomainException("O ID do produto é obrigatório.");
        }

        // Valida que o ID da empresa não é vazio
        if (companyId == Guid.Empty)
        {
            // Lança exceção informando que a empresa é obrigatória
            throw new DomainException("O ID da empresa é obrigatório.");
        }

        // Gera um identificador único para o registro de preço customizado
        Id = Guid.NewGuid();

        // Atribui os identificadores validados
        ProductId = productId;
        CompanyId = companyId;

        // Valida e define o preço customizado e a quantidade mínima
        SetCustomPrice(customPrice);
        SetMinQuantity(minQuantity);

        // Todo preço customizado novo é criado como ativo
        IsActive = true;
    }

    // Identificador único do registro de preço customizado
    public Guid Id { get; private set; }

    // Chave estrangeira: ID do produto ao qual o preço se aplica
    public Guid ProductId { get; private set; }

    // Chave estrangeira: ID da empresa compradora que recebe este preço especial
    public Guid CompanyId { get; private set; }

    // Preço customizado negociado para esta empresa específica
    // Armazenado como decimal para precisão monetária (evita erros de arredondamento)
    public decimal CustomPrice { get; private set; }

    // Quantidade mínima de unidades para que este preço seja aplicado
    // Ex: preço especial só vale para compras de 100+ unidades
    public int MinQuantity { get; private set; }

    // Indica se este preço customizado está ativo e pode ser aplicado
    public bool IsActive { get; private set; }

    // Propriedade de navegação: produto ao qual o preço pertence
    // virtual permite lazy loading pelo EF Core
    public virtual Product Product { get; private set; } = null!;

    // Propriedade de navegação: empresa compradora que tem este preço especial
    public virtual Company Company { get; private set; } = null!;

    // Atualiza o valor do preço customizado com validação
    // O preço deve ser maior que zero (não faz sentido preço zero ou negativo)
    public void SetCustomPrice(decimal customPrice)
    {
        // Valida que o preço é estritamente maior que zero
        // Preço zero ou negativo representaria um erro de negócio
        if (customPrice <= 0)
        {
            // Lança exceção informando que o preço deve ser positivo
            throw new DomainException("O preço customizado deve ser maior que zero.");
        }

        // Atribui o preço validado
        CustomPrice = customPrice;
    }

    // Atualiza a quantidade mínima para aplicação do preço
    // A quantidade mínima deve ser pelo menos 1 (não faz sentido comprar 0 unidades)
    public void SetMinQuantity(int minQuantity)
    {
        // Valida que a quantidade mínima é pelo menos 1
        if (minQuantity < 1)
        {
            // Lança exceção informando que a quantidade mínima é 1
            throw new DomainException("A quantidade mínima deve ser pelo menos 1.");
        }

        // Atribui a quantidade mínima validada
        MinQuantity = minQuantity;
    }

    // Ativa o preço customizado, permitindo que ele seja aplicado novamente
    public void Activate()
    {
        // Define a flag IsActive como true (ativo)
        IsActive = true;
    }

    // Desativa o preço customizado, suspendendo temporariamente o preço especial
    // Soft delete: mantém o registro para histórico de negociações
    public void Deactivate()
    {
        // Define a flag IsActive como false (inativo)
        IsActive = false;
    }
}
