// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para os builders de configuração
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace para as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade OrderItem
// Define o mapeamento da tabela de itens de pedido (linhas do pedido)
// Cada item representa um produto específico dentro de um pedido, com quantidade e preços
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    // Aplica as configurações de mapeamento da entidade OrderItem
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Define o nome da tabela no banco de dados
        builder.ToTable("OrderItems");

        // Configura a chave primária
        builder.HasKey(oi => oi.Id);

        // Configura a precisão da propriedade UnitPrice (preço unitário no momento da compra)
        // HasPrecision(18,2) - precisão monetária padrão
        // O preço é congelado no momento do pedido para evitar que alterações futuras
        // no produto afetem pedidos já realizados
        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);        // Precisão monetária: 18 dígitos totais, 2 decimais

        // Configura a precisão da propriedade TotalPrice (preço total do item)
        // TotalPrice = UnitPrice * Quantity (calculado no construtor da entidade)
        // HasPrecision(18,2) - mesma precisão do UnitPrice
        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(18, 2);        // Precisão monetária: 18 dígitos totais, 2 decimais

        // Configura o relacionamento com a entidade Order (pedido)
        // HasOne<Order>() - cada item pertence a um pedido
        // WithMany(o => o.Items) - um pedido pode ter muitos itens
        // OnDelete(Cascade) - se o pedido for excluído, todos os seus itens também são
        // Cascade é apropriado: itens não fazem sentido sem o pedido ao qual pertencem
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)      // Order.Items é a coleção de itens do pedido
            .HasForeignKey(oi => oi.OrderId) // Chave estrangeira para o pedido
            .OnDelete(DeleteBehavior.Cascade); // Exclui itens quando o pedido é excluído

        // Configura o relacionamento com a entidade Product
        // HasOne<Product>() - cada item referencia um produto do catálogo
        // WithMany() - Product não tem coleção de OrderItems
        // OnDelete(Restrict) - NÃO permite excluir um produto que esteja em algum pedido
        // Isso preserva o histórico: pedidos passados continuam referenciando o produto correto
        builder.HasOne(oi => oi.Product)
            .WithMany()                  // Product não tem propriedade de navegação para OrderItem
            .HasForeignKey(oi => oi.ProductId) // Chave estrangeira para o produto
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão do produto se estiver em pedidos
    }
}
