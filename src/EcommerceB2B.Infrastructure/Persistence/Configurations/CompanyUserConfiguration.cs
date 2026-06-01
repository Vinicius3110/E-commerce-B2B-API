// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Identity para referenciar a tabela de usuários (IdentityUser)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace para as configurações de mapeamento Fluent API
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade CompanyUser
// Define o mapeamento da tabela associativa entre usuários e empresas
// Um usuário pode pertencer a múltiplas empresas (relacionamento many-to-many)
public class CompanyUserConfiguration : IEntityTypeConfiguration<CompanyUser>
{
    // Método que aplica as configurações de mapeamento via Fluent API
    // A Fluent API é mais expressiva que Data Annotations para cenários complexos
    public void Configure(EntityTypeBuilder<CompanyUser> builder)
    {
        // Define o nome da tabela no banco de dados
        builder.ToTable("CompanyUsers");

        // Configura a chave primária (Id)
        // O EF Core usa a convenção: propriedade chamada "Id" ou "<NomeClasse>Id" é PK
        // Mas declaramos explicitamente para documentar e evitar ambiguidades
        builder.HasKey(cu => cu.Id);

        // Configura a propriedade UserId (chave estrangeira para AspNetUsers)
        // IsRequired() - todo vínculo precisa de um usuário
        builder.Property(cu => cu.UserId)
            .IsRequired();               // Campo obrigatório (NOT NULL)

        // Configura a propriedade CompanyId (chave estrangeira para Companies)
        // IsRequired() - todo vínculo precisa de uma empresa
        builder.Property(cu => cu.CompanyId)
            .IsRequired();               // Campo obrigatório (NOT NULL)

        // Cria um índice único composto por UserId e CompanyId
        // Garante que um mesmo usuário não pode ser vinculado duas vezes à mesma empresa
        // Isso é uma restrição de integridade em nível de banco de dados
        builder.HasIndex(cu => new { cu.UserId, cu.CompanyId })
            .IsUnique();                 // Índice único composto

        // Configura o relacionamento entre CompanyUser e IdentityUser
        // HasOne<IdentityUser<Guid>>() - cada CompanyUser pertence a um usuário
        // WithMany() - um usuário pode ter múltiplos vínculos (com várias empresas)
        // HasForeignKey(UserId) - a FK que referencia o usuário é UserId
        // OnDelete(DeleteBehavior.Restrict) - NÃO permite excluir um usuário que tenha vínculos
        // Restrict é mais seguro que Cascade: evita exclusão acidental de dados relacionados
        builder.HasOne<IdentityUser<Guid>>()
            .WithMany()                  // IdentityUser não tem propriedade de navegação para CompanyUser
            .HasForeignKey(cu => cu.UserId) // Chave estrangeira na tabela CompanyUsers
            .OnDelete(DeleteBehavior.Restrict); // Restringe exclusão se houver vínculos ativos

        // Configura o valor padrão para IsActive
        // Vínculos novos são criados como ativos por padrão
        builder.Property(cu => cu.IsActive)
            .HasDefaultValue(true);      // Valor padrão: vínculo ativo
    }
}
