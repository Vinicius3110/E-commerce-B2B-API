// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para os builders de configuração
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace para as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade ProductPrice
// Define o mapeamento da tabela de preços customizados por empresa compradora
// Implementa o modelo de precificação B2B: cada comprador pode negociar preços diferentes
public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    // Aplica as configurações de mapeamento via Fluent API
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        // Define o nome da tabela no banco de dados
        builder.ToTable("ProductPrices");

        // Configura a chave primária
        builder.HasKey(pp => pp.Id);

        // Configura a precisão da propriedade CustomPrice (preço negociado para o comprador)
        // HasPrecision(18,2) - mesma precisão do preço base do produto
        // 18 dígitos totais, 2 casas decimais para valores monetários
        builder.Property(pp => pp.CustomPrice)
            .HasPrecision(18, 2);        // Precisão monetária: 18 dígitos totais, 2 decimais

        // Cria um índice único composto por ProductId e CompanyId
        // Garante que não pode haver dois preços diferentes para a mesma empresa no mesmo produto
        // Cada empresa compradora tem no máximo um preço customizado por produto
        builder.HasIndex(pp => new { pp.ProductId, pp.CompanyId })
            .IsUnique();                 // Índice único composto

        // Configura o relacionamento com a entidade Product
        // HasOne<Product>() - cada preço customizado pertence a um produto
        // WithMany(p => p.CustomPrices) - um produto pode ter vários preços customizados
        // OnDelete(Cascade) - se o produto for excluído, seus preços customizados também são
        // Cascade é apropriado aqui: não faz sentido manter preços de um produto que não existe
        builder.HasOne(pp => pp.Product)
            .WithMany(p => p.CustomPrices) // Product.CustomPrices é a propriedade de navegação reversa
            .HasForeignKey(pp => pp.ProductId) // Chave estrangeira
            .OnDelete(DeleteBehavior.Cascade); // Exclui preços customizados quando o produto é excluído

        // Configura o relacionamento com a entidade Company (empresa compradora)
        // HasOne<Company>() - cada preço é concedido a uma empresa específica
        // WithMany() - Company não tem coleção de preços customizados (evita carregar tudo)
        // OnDelete(Restrict) - NÃO permite excluir empresa que tenha preços customizados
        // Diferente do produto, a empresa é uma entidade importante que não deve ser excluída se tem dados
        builder.HasOne(pp => pp.Company)
            .WithMany()                  // Company não tem propriedade de navegação para ProductPrice
            .HasForeignKey(pp => pp.CompanyId) // Chave estrangeira
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão se houver preços vinculados

        // Configura o valor padrão para IsActive
        // Preços customizados novos são criados como ativos por padrão
        builder.Property(pp => pp.IsActive)
            .HasDefaultValue(true);      // Valor padrão: preço ativo
    }
}
