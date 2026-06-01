// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para os builders de configuração
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace para as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade Order
// Define o mapeamento da tabela de pedidos, incluindo relacionamentos com comprador e vendedor
// Um pedido envolve duas empresas: uma que compra e outra que vende
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    // Aplica as configurações de mapeamento da entidade Order
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Define o nome da tabela no banco de dados
        builder.ToTable("Orders");

        // Configura a chave primária
        builder.HasKey(o => o.Id);

        // Configura a propriedade Status (status do pedido no fluxo B2B)
        // HasConversion<byte>() converte o enum OrderStatus para byte no banco
        // OrderStatus é declarado como enum : byte, então ocupa apenas 1 byte (TINYINT)
        // Os valores são: Pendente=1, Confirmado=2, Cancelado=3, Enviado=4, Entregue=5
        builder.Property(o => o.Status)
            .HasConversion<byte>();      // Converte o enum para byte (TINYINT)

        // Configura a precisão da propriedade TotalAmount (valor total do pedido)
        // HasPrecision(18,2) - precisão monetária padrão
        // TotalAmount é calculado como a soma dos TotalPrice de todos os OrderItems
        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);        // Precisão monetária: 18 dígitos totais, 2 decimais

        // Configura o relacionamento com a empresa compradora (BuyerCompany)
        // HasOne<Company>(BuyerCompany) - cada pedido tem uma empresa compradora
        // WithMany() - uma empresa pode fazer muitos pedidos como compradora
        // OnDelete(Restrict) - NÃO permite excluir empresa que tenha pedidos como compradora
        // Isso preserva o histórico de compras para auditoria
        builder.HasOne(o => o.BuyerCompany)
            .WithMany()                  // Company não tem coleção de pedidos como compradora
            .HasForeignKey(o => o.BuyerCompanyId) // Chave estrangeira para a empresa compradora
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão se houver pedidos

        // Configura o relacionamento com a empresa vendedora (SellerCompany)
        // HasOne<Company>(SellerCompany) - cada pedido tem uma empresa vendedora
        // WithMany() - uma empresa pode ter muitos pedidos como vendedora
        // OnDelete(Restrict) - NÃO permite excluir empresa que tenha pedidos como vendedora
        // Ambas as FKs usam Restrict porque empresas são entidades centrais do sistema
        builder.HasOne(o => o.SellerCompany)
            .WithMany()                  // Company não tem coleção de pedidos como vendedora
            .HasForeignKey(o => o.SellerCompanyId) // Chave estrangeira para a empresa vendedora
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão se houver pedidos
    }
}
