// Define o namespace do DTO dentro da camada de Aplicacao, na area de Empresa
namespace EcommerceB2B.Application.DTOs.Company;

/// <summary>
/// DTO que representa os dados de uma empresa para leitura (retorno em consultas).
/// Utilizado para transferir dados do dominio para a camada de apresentacao sem expor a entidade diretamente.
/// </summary>
public class CompanyDto
{
    /// <summary>
    /// Identificador unico global (GUID) da empresa no sistema.
    /// Usado como chave primaria na tabela de empresas.
    /// </summary>
    public Guid Id { get; set; } // Identificador unico da empresa (UUID)

    /// <summary>
    /// Nome comercial (razao social ou nome fantasia) da empresa.
    /// Exibido nas listagens e detalhes da plataforma B2B.
    /// </summary>
    public string Name { get; set; } = string.Empty; // Nome da empresa para exibicao

    /// <summary>
    /// Documento oficial da empresa (CNPJ).
    /// Armazenado sem mascara, contendo apenas os digitos numericos.
    /// </summary>
    public string Document { get; set; } = string.Empty; // CNPJ da empresa (somente numeros)

    /// <summary>
    /// Tipo da empresa: "Supplier" (fornecedora) ou "Buyer" (compradora).
    /// Define o papel da empresa na plataforma e quais funcionalidades estao disponiveis.
    /// </summary>
    public string Type { get; set; } = string.Empty; // Tipo: Supplier (fornecedor) ou Buyer (comprador)

    /// <summary>
    /// Indica se a empresa esta ativa no sistema.
    /// Empresas inativas nao podem acessar a plataforma ou realizar transacoes.
    /// </summary>
    public bool IsActive { get; set; } // Status de atividade da empresa (true = ativa, false = inativa)

    /// <summary>
    /// Data e hora (UTC) em que a empresa foi cadastrada no sistema.
    /// Utilizada para auditoria e ordenacao de registros.
    /// </summary>
    public DateTime CreatedAt { get; set; } // Data de criacao do registro da empresa (timestamp UTC)
}
