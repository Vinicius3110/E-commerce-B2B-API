// Importa as entidades do domínio que serão mapeadas para tabelas
using EcommerceB2B.Domain.Entities;

// Importa o Identity para configurar as tabelas de autenticação
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// Namespace que organiza as classes de persistência (banco de dados)
namespace EcommerceB2B.Infrastructure.Persistence;

// Classe principal de contexto do banco de dados
// Herda de IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid> para integrar
// o ASP.NET Core Identity com o Entity Framework Core usando Guids como chaves primárias
// IdentityDbContext já inclui DbSets para Users, Roles, Claims, Tokens e Logins
public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    // DbSet representa uma tabela no banco de dados
    // Cada DbSet é uma coleção que o EF Core usa para fazer queries e salvar dados

    // Tabela de empresas (tenants do sistema multi-tenant)
    // Armazena compradores e vendedores da plataforma B2B
    public DbSet<Company> Companies { get; set; } = null!;

    // Tabela de vínculo entre usuários do Identity e empresas
    // Permite que um usuário esteja associado a múltiplas empresas
    public DbSet<CompanyUser> CompanyUsers { get; set; } = null!;

    // Tabela de categorias de produtos
    // Organiza produtos em grupos para facilitar navegação e busca
    public DbSet<Category> Categories { get; set; } = null!;

    // Tabela de produtos anunciados na plataforma
    // Cada produto pertence a uma empresa vendedora e a uma categoria
    public DbSet<Product> Products { get; set; } = null!;

    // Tabela de preços customizados por empresa compradora
    // Permite negociação B2B: cada comprador pode ter preços diferentes para o mesmo produto
    public DbSet<ProductPrice> ProductPrices { get; set; } = null!;

    // Tabela de pedidos de compra entre empresas
    // Gerencia o ciclo de vida completo do pedido B2B
    public DbSet<Order> Orders { get; set; } = null!;

    // Tabela de itens de pedido (linhas do pedido)
    // Cada item referencia um produto, quantidade e preços
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    // Construtor que recebe as opções de configuração do DbContext
    // DbContextOptions<AppDbContext> contém a string de conexão e outras configs
    // : base(options) passa as opções para a classe base IdentityDbContext
    // Isso garante que o Identity configure corretamente suas tabelas internas
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Método chamado pelo EF Core durante a criação do modelo
    // Aqui configuramos mapeamentos adicionais que não podem ser feitos por Data Annotations
    // É o ponto central onde todas as configurações de entidade são aplicadas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chama a configuração da classe base primeiro
        // Isso é OBRIGATÓRIO para que o Identity configure suas próprias tabelas
        // (AspNetUsers, AspNetRoles, AspNetUserClaims, AspNetUserTokens, AspNetUserLogins, etc.)
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações de entidade definidas em classes separadas
        // ApplyConfigurationsFromAssembly escaneia o assembly atual e aplica todas as classes
        // que implementam IEntityTypeConfiguration<TEntity>
        // Isso mantém o DbContext limpo e cada entidade com sua configuração em arquivo próprio
        // Princípio de Responsabilidade Única (SRP): o DbContext não precisa saber os detalhes
        // de mapeamento de cada entidade
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
