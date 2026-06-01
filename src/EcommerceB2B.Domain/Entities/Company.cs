// Namespace que agrupa todas as entidades do domínio B2B
namespace EcommerceB2B.Domain.Entities;

// Importa as exceções customizadas do domínio para validação de regras de negócio
using EcommerceB2B.Domain.Exceptions;

// Importa os tipos enumerados do domínio (CompanyType) para definir o tipo de empresa
using EcommerceB2B.Domain.Enums;

// A classe Company representa uma empresa na plataforma B2B
// É uma entidade de domínio rica (rich domain model) que encapsula suas regras de negócio
// Toda alteração de estado passa por métodos que validam as regras
public class Company
{
    // Construtor privado sem parâmetros para uso exclusivo do Entity Framework Core
    // O EF Core precisa deste construtor para materializar objetos vindos do banco de dados
    // É privado para impedir que código externo crie instâncias em estado inválido
    private Company()
    {
    }

    // Construtor público: única forma de criar uma nova empresa no sistema
    // Recebe os dados obrigatórios e aplica as regras de validação imediatamente
    // O parâmetro type tem valor padrão CompanyType.Ambos (empresa que compra e vende)
    public Company(string name, string document, CompanyType type = CompanyType.Ambos)
    {
        // Gera um identificador único universal (UUID) para a empresa
        // Guid.NewGuid() garante unicidade global sem depender do banco de dados
        Id = Guid.NewGuid();

        // Valida e define o nome da empresa usando o método SetName
        // Centraliza a validação no método para evitar duplicação de código
        SetName(name);

        // Valida e define o documento (CNPJ) da empresa
        SetDocument(document);

        // Define o tipo da empresa (Comprador, Vendedor ou Ambos)
        // A validação ocorre dentro do método SetType
        SetType(type);

        // Toda empresa nova começa como ativa no sistema
        IsActive = true;

        // Registra a data/hora de criação em UTC para consistência global
        // DateTime.UtcNow evita problemas de fuso horário entre regiões
        CreatedAt = DateTime.UtcNow;
    }

    // Identificador único da empresa (chave primária)
    // Guid é ideal para sistemas distribuídos pois não depende de sequência autoincremental
    // private set garante encapsulamento: só a própria entidade pode alterar o Id
    public Guid Id { get; private set; }

    // Nome fantasia ou razão social da empresa
    // private set protege contra alterações externas não validadas
    public string Name { get; private set; } = null!;

    // Documento da empresa (CNPJ) usado para identificação fiscal
    // O null! suprime o warning de CS8618 pois garantimos a inicialização via construtor
    public string Document { get; private set; } = null!;

    // Tipo da empresa: Comprador=1, Vendedor=2, Ambos=3
    // Define o papel da empresa na plataforma e quais operações ela pode realizar
    public CompanyType Type { get; private set; }

    // Indica se a empresa está ativa no sistema
    // Empresas inativas não podem realizar transações (soft delete lógico)
    public bool IsActive { get; private set; }

    // Data e hora de criação do registro (UTC)
    // Útil para auditoria, relatórios e ordenação cronológica
    public DateTime CreatedAt { get; private set; }

    // Atualiza o nome da empresa com validação de regra de negócio
    // O nome é obrigatório e não pode ser vazio ou apenas espaços em branco
    public void SetName(string name)
    {
        // Verifica se o nome é nulo, vazio ou contém apenas espaços
        // string.IsNullOrWhiteSpace cobre os três casos em uma única verificação
        if (string.IsNullOrWhiteSpace(name))
        {
            // Lança exceção de domínio com mensagem em português (pt-br)
            // A mensagem descreve claramente o erro para facilitar o entendimento
            throw new DomainException("O nome da empresa é obrigatório.");
        }

        // Atribui o nome validado à propriedade
        Name = name;
    }

    // Atualiza o documento (CNPJ) da empresa com validação
    // O documento é obrigatório para identificação fiscal da empresa
    public void SetDocument(string document)
    {
        // Valida que o documento não é nulo, vazio ou apenas espaços
        if (string.IsNullOrWhiteSpace(document))
        {
            // Informa que o documento (CNPJ) é obrigatório
            throw new DomainException("O documento (CNPJ) da empresa é obrigatório.");
        }

        // Atribui o documento validado à propriedade
        Document = document;
    }

    // Altera o tipo da empresa (Comprador, Vendedor ou Ambos)
    // Permite que uma empresa mude seu papel na plataforma ao longo do tempo
    public void SetType(CompanyType type)
    {
        // Verifica se o valor informado é um valor válido do enum CompanyType
        // Enum.IsDefined garante que apenas valores definidos no enum são aceitos
        if (!Enum.IsDefined(typeof(CompanyType), type))
        {
            // Rejeita valores inválidos que não correspondem a nenhuma opção do enum
            throw new DomainException("O tipo de empresa informado é inválido.");
        }

        // Atribui o tipo validado à propriedade
        Type = type;
    }

    // Ativa a empresa, permitindo que ela volte a operar na plataforma
    // Útil para reativar empresas que foram temporariamente desativadas
    public void Activate()
    {
        // Define a flag IsActive como true (ativa)
        IsActive = true;
    }

    // Desativa a empresa, impedindo que ela realize transações
    // Implementa soft delete: o registro permanece no banco, mas fica inativo
    public void Deactivate()
    {
        // Define a flag IsActive como false (inativa)
        IsActive = false;
    }
}
