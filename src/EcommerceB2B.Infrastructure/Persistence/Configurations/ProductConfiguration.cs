// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para os builders de configuração
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace que organiza as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade Product
// Define o mapeamento da tabela de produtos, incluindo índices, relacionamentos e precisão monetária
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // Aplica as configurações de mapeamento da entidade Product
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Define o nome da tabela no banco de dados
        builder.ToTable("Products");

        // Configura a chave primária
        builder.HasKey(p => p.Id);

        // Configura a propriedade Name (nome do produto)
        // IsRequired() - todo produto precisa de um nome para identificação no catálogo
        // HasMaxLength(200) - limite para nomes descritivos como "Notebook Dell XPS 15"
        builder.Property(p => p.Name)
            .IsRequired()               // Campo obrigatório (NOT NULL)
            .HasMaxLength(200);          // Tamanho máximo de 200 caracteres

        // Configura a propriedade Description (descrição detalhada do produto)
        // Campo opcional (string? na entidade), sem IsRequired()
        // HasMaxLength(2000) - permite descrições longas com especificações técnicas
        builder.Property(p => p.Description)
            .HasMaxLength(2000);         // Tamanho máximo de 2000 caracteres (campo opcional)

        // Configura a propriedade Sku (Stock Keeping Unit - código único de estoque)
        // IsRequired() - SKU é essencial para controle de inventário
        // HasMaxLength(50) - limite para códigos SKU alfanuméricos
        builder.Property(p => p.Sku)
            .IsRequired()               // Campo obrigatório (NOT NULL)
            .HasMaxLength(50);           // Tamanho máximo de 50 caracteres

        // Configura a precisão da propriedade BasePrice (preço base do produto)
        // HasPrecision(18,2) define 18 dígitos no total, sendo 2 casas decimais
        // Exemplo: 9999999999999999.99 é o valor máximo
        // decimal é essencial para valores monetários: evita erros de arredondamento do float/double
        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 2);        // Precisão monetária: 18 dígitos totais, 2 casas decimais

        // Cria um índice único composto por CompanyId e Sku
        // Garante que uma mesma empresa não pode ter dois produtos com o mesmo SKU
        // Mas empresas diferentes podem ter SKUs iguais (cada empresa tem seu próprio catálogo)
        builder.HasIndex(p => new { p.CompanyId, p.Sku })
            .IsUnique();                 // Índice único composto

        // Configura o relacionamento com a entidade Company (empresa vendedora)
        // HasOne<Company>() - cada produto pertence a uma empresa
        // WithMany() - uma empresa pode ter muitos produtos (não há propriedade de navegação reversa)
        // HasForeignKey(CompanyId) - a FK na tabela Products que referencia Companies
        // OnDelete(Restrict) - NÃO permite excluir uma empresa que tenha produtos cadastrados
        // Isso protege o catálogo: produtos não podem ficar órfãos
        builder.HasOne(p => p.Company)
            .WithMany()                  // Company não tem coleção de Products (evita carregar tudo)
            .HasForeignKey(p => p.CompanyId) // Chave estrangeira
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão da empresa se houver produtos

        // Configura o relacionamento com a entidade Category
        // HasOne<Category>() - cada produto pertence a uma categoria
        // WithMany(c => c.Products) - uma categoria tem muitos produtos (propriedade de navegação)
        // OnDelete(Restrict) - NÃO permite excluir uma categoria que tenha produtos vinculados
        // Isso preserva a integridade referencial: produtos não ficam sem categoria
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)   // Category.Products é a propriedade de navegação reversa
            .HasForeignKey(p => p.CategoryId) // Chave estrangeira
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão da categoria se houver produtos

        // Configura o valor padrão para IsActive
        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);      // Valor padrão: produto ativo
    }
}
