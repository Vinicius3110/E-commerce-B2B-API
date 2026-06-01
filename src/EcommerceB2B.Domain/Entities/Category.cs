// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// A classe Category representa uma categoria de produtos na plataforma
// Permite organizar produtos em grupos hierárquicos (ex: Eletrônicos, Vestuário, Alimentos)
// Facilita a navegação e busca de produtos no catálogo B2B
public class Category
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    private Category()
    {
    }

    // Construtor público: cria uma nova categoria com nome obrigatório e descrição opcional
    // A descrição é opcional (string?) para casos onde o nome já é autoexplicativo
    public Category(string name, string? description = null)
    {
        // Gera um identificador único para a categoria
        Id = Guid.NewGuid();

        // Valida e define o nome da categoria usando o método SetName
        SetName(name);

        // Define a descrição (pode ser nula, não requer validação obrigatória)
        Description = description;

        // Toda categoria nova é criada como ativa
        IsActive = true;
    }

    // Identificador único da categoria
    public Guid Id { get; private set; }

    // Nome da categoria (ex: "Eletrônicos", "Material de Escritório")
    // É o principal atributo de identificação para o usuário final
    public string Name { get; private set; } = null!;

    // Descrição opcional da categoria para fornecer mais contexto
    // O tipo string? indica que o valor pode ser nulo no banco de dados
    public string? Description { get; private set; }

    // Indica se a categoria está ativa no sistema
    // Categorias inativas não aparecem no catálogo, mas mantêm produtos vinculados
    public bool IsActive { get; private set; }

    // Propriedade de navegação: coleção de produtos pertencentes a esta categoria
    // virtual permite lazy loading pelo EF Core (carrega produtos só quando acessados)
    // ICollection é a interface recomendada para coleções de navegação no EF Core
    // Inicializa como HashSet para evitar duplicatas e ter busca O(1)
    public virtual ICollection<Product> Products { get; private set; } = new HashSet<Product>();

    // Atualiza o nome da categoria com validação
    // O nome é obrigatório pois é como o usuário identifica a categoria
    public void SetName(string name)
    {
        // Valida que o nome não é nulo, vazio ou apenas espaços em branco
        if (string.IsNullOrWhiteSpace(name))
        {
            // Lança exceção informando que o nome da categoria é obrigatório
            throw new DomainException("O nome da categoria é obrigatório.");
        }

        // Atribui o nome validado à propriedade
        Name = name;
    }

    // Atualiza a descrição da categoria
    // A descrição é opcional: pode ser definida como null para removê-la
    public void SetDescription(string? description)
    {
        // Atribui a descrição sem validação obrigatória (campo opcional)
        // string? aceita null, permitindo remover a descrição existente
        Description = description;
    }

    // Ativa a categoria, tornando-a visível no catálogo novamente
    public void Activate()
    {
        // Define a flag IsActive como true (ativa)
        IsActive = true;
    }

    // Desativa a categoria, ocultando-a do catálogo
    // Implementa soft delete: o registro e seus produtos permanecem no banco
    public void Deactivate()
    {
        // Define a flag IsActive como false (inativa)
        IsActive = false;
    }
}
