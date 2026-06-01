// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// A classe CompanyUser representa o vínculo entre um usuário e uma empresa
// Funciona como uma tabela associativa (many-to-many) entre User e Company
// Permite que um mesmo usuário tenha acesso a múltiplas empresas
public class CompanyUser
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    // O EF Core utiliza este construtor ao materializar entidades a partir do banco
    private CompanyUser()
    {
    }

    // Construtor público: cria um vínculo entre um usuário e uma empresa
    // Ambos os identificadores são obrigatórios e devem ser válidos
    public CompanyUser(Guid userId, Guid companyId)
    {
        // Valida que o ID do usuário não é um Guid vazio (00000000-0000-0000-0000-000000000000)
        // Garante que o vínculo sempre referencia um usuário real existente
        if (userId == Guid.Empty)
        {
            // Lança exceção informando que o ID do usuário é obrigatório
            throw new DomainException("O ID do usuário é obrigatório.");
        }

        // Valida que o ID da empresa não é um Guid vazio
        if (companyId == Guid.Empty)
        {
            // Lança exceção informando que o ID da empresa é obrigatório
            throw new DomainException("O ID da empresa é obrigatório.");
        }

        // Gera um identificador único para este vínculo
        Id = Guid.NewGuid();

        // Atribui os identificadores validados às propriedades
        UserId = userId;
        CompanyId = companyId;

        // Todo vínculo novo é criado como ativo
        IsActive = true;
    }

    // Identificador único do vínculo usuário-empresa
    // private set garante que o Id só pode ser definido internamente
    public Guid Id { get; private set; }

    // Chave estrangeira que referencia o usuário no sistema
    // Representa o lado do "muitos" na relação User -> CompanyUser
    public Guid UserId { get; private set; }

    // Chave estrangeira que referencia a empresa no sistema
    // Representa o lado do "muitos" na relação Company -> CompanyUser
    public Guid CompanyId { get; private set; }

    // Indica se o vínculo está ativo (usuário pode acessar a empresa)
    // Permite desvincular temporariamente sem excluir o registro
    public bool IsActive { get; private set; }

    // Propriedade de navegação para a entidade Company associada
    // virtual permite que o EF Core faça lazy loading (carregamento sob demanda)
    // O null! suprime o warning pois o EF Core preencherá esta propriedade
    public virtual Company Company { get; private set; } = null!;

    // Ativa o vínculo, permitindo que o usuário volte a acessar a empresa
    public void Activate()
    {
        // Define a flag IsActive como true (vínculo ativo)
        IsActive = true;
    }

    // Desativa o vínculo, removendo temporariamente o acesso do usuário à empresa
    // O registro permanece no banco para fins de auditoria e histórico
    public void Deactivate()
    {
        // Define a flag IsActive como false (vínculo inativo)
        IsActive = false;
    }
}
