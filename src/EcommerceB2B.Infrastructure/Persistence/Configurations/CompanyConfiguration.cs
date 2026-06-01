// Importa a entidade de domínio que será configurada
using EcommerceB2B.Domain.Entities;

// Importa o Entity Framework Core para configuração do modelo
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Namespace que organiza as configurações de mapeamento de entidades
// Cada arquivo contém a configuração Fluent API de uma única entidade (SRP)
namespace EcommerceB2B.Infrastructure.Persistence.Configurations;

// Classe de configuração para a entidade Company
// Implementa IEntityTypeConfiguration<Company> para separar o mapeamento do DbContext
// Isso mantém o DbContext limpo e cada entidade com suas regras em arquivo próprio
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    // Método chamado pelo EF Core para aplicar as configurações de mapeamento
    // Recebe um builder tipado que permite configurar colunas, índices, relacionamentos, etc.
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        // Define o nome da tabela no banco de dados como "Companies"
        // Por convenção o EF Core usaria o mesmo nome, mas definimos explicitamente
        // para deixar claro e evitar surpresas com pluralizações automáticas
        builder.ToTable("Companies");

        // Configura a chave primária da tabela
        // HasKey define qual propriedade é a PK (Primary Key)
        builder.HasKey(c => c.Id);

        // Configura a propriedade Name (nome/razão social da empresa)
        // IsRequired() torna a coluna NOT NULL no banco de dados
        // HasMaxLength(200) limita o tamanho a 200 caracteres (equivalente a NVARCHAR(200))
        builder.Property(c => c.Name)
            .IsRequired()               // Campo obrigatório (NOT NULL)
            .HasMaxLength(200);          // Tamanho máximo de 200 caracteres

        // Configura a propriedade Document (CNPJ da empresa)
        // IsRequired() - documento é obrigatório para identificação fiscal
        // HasMaxLength(14) - CNPJ tem exatamente 14 dígitos numéricos
        builder.Property(c => c.Document)
            .IsRequired()               // Campo obrigatório (NOT NULL)
            .HasMaxLength(14);          // CNPJ tem 14 dígitos

        // Cria um índice único para o campo Document
        // IsUnique() garante que não pode haver duas empresas com o mesmo CNPJ
        // Isso impõe a regra de unicidade diretamente no banco de dados
        builder.HasIndex(c => c.Document)
            .IsUnique();                 // Índice único (UNIQUE INDEX)

        // Configura a propriedade Type (tipo da empresa: Comprador, Vendedor, Ambos)
        // HasConversion<byte>() converte o enum CompanyType para byte no banco
        // CompanyType é declarado como enum : byte, então usa 1 byte de armazenamento
        // Isso é mais eficiente que armazenar como string ou int (4 bytes)
        builder.Property(c => c.Type)
            .HasConversion<byte>();      // Converte o enum para byte (TINYINT no PostgreSQL)

        // Configura a propriedade IsActive com valor padrão true
        // HasDefaultValue(true) faz o banco usar true como valor padrão ao inserir
        // Isso significa que novas empresas são ativas por default mesmo sem especificar
        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);      // Valor padrão: empresa ativa
    }
}
