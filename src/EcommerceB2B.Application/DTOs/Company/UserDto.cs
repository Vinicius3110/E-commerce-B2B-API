// Define o namespace do DTO dentro da camada de Aplicacao, na area de Empresa
namespace EcommerceB2B.Application.DTOs.Company;

/// <summary>
/// DTO que representa os dados de um usuario para leitura (retorno em consultas).
/// Exibe informacoes resumidas do usuario, omitindo dados sensiveis como senha.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Identificador unico global (GUID) do usuario no sistema.
    /// Usado como chave primaria na tabela de usuarios (Identity).
    /// </summary>
    public Guid Id { get; set; } // Identificador unico do usuario (UUID)

    /// <summary>
    /// Nome completo do usuario conforme cadastrado no sistema.
    /// </summary>
    public string Name { get; set; } = string.Empty; // Nome de exibicao do usuario

    /// <summary>
    /// Endereco de e-mail do usuario.
    /// Utilizado como identificador unico para login no sistema.
    /// </summary>
    public string Email { get; set; } = string.Empty; // E-mail do usuario para contato e login

    /// <summary>
    /// Perfil (role) atribuido ao usuario.
    /// Define as permissoes e acessos do usuario dentro da plataforma.
    /// </summary>
    public string Role { get; set; } = string.Empty; // Perfil de acesso do usuario (Admin, Manager, etc.)

    /// <summary>
    /// Indica se o usuario esta ativo e pode acessar o sistema.
    /// Usuarios inativos nao conseguem fazer login.
    /// </summary>
    public bool IsActive { get; set; } // Status de atividade do usuario (true = ativo, false = bloqueado)
}
