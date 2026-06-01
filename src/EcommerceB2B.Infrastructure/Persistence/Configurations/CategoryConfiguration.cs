// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para os builders de configuração
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace que organiza as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade Category
// Define como a entidade Category é mapeada para a tabela "Categories" no banco
// Categorias organizam produtos em grupos lógicos para navegação no catálogo
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // Aplica as regras de mapeamento da entidade para o banco de dados
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Define o nome da tabela explicitamente
        builder.ToTable("Categories");

        // Configura a chave primária da tabela
        builder.HasKey(c => c.Id);

        // Configura a propriedade Name (nome da categoria)
        // IsRequired() - toda categoria precisa de um nome para identificação
        // HasMaxLength(100) - limite prático para nomes de categoria (ex: "Material de Escritório")
        builder.Property(c => c.Name)
            .IsRequired()               // Campo obrigatório (NOT NULL)
            .HasMaxLength(100);          // Tamanho máximo de 100 caracteres

        // Configura a propriedade Description (descrição da categoria)
        // Esta propriedade é string? na entidade (opcional), então não usamos IsRequired()
        // HasMaxLength(500) - limite para descrições concisas mas informativas
        builder.Property(c => c.Description)
            .HasMaxLength(500);          // Tamanho máximo de 500 caracteres (campo opcional)

        // Configura o valor padrão para IsActive
        // Categorias novas são criadas como ativas por padrão
        // Isso evita que categorias fiquem inativas acidentalmente
        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);      // Valor padrão: categoria ativa
    }
}
