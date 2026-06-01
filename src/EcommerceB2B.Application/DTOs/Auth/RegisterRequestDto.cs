// Importa o namespace para usar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado para receber os dados de cadastro (registro) de uma nova empresa e seu administrador.
/// Todas as propriedades possuem validacao via Data Annotations para garantir a integridade dos dados recebidos.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// Nome da empresa que esta sendo cadastrada.
    /// Campo obrigatorio com limite maximo de 200 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome da empresa é obrigatório.")] // Indica que o campo e obrigatorio; exibe mensagem personalizada se nulo/vazio
    [MaxLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres.")] // Limita a quantidade maxima de caracteres para 200
    public string CompanyName { get; set; } = string.Empty; // Inicializa com string vazia para evitar valores nulos (null safety)

    /// <summary>
    /// Documento da empresa (CNPJ) contendo apenas os digitos numericos.
    /// Campo obrigatorio com exatamente 14 digitos numericos.
    /// </summary>
    [Required(ErrorMessage = "O documento (CNPJ) é obrigatório.")] // Valida que o campo nao sera nulo ou vazio
    [MaxLength(14, ErrorMessage = "O CNPJ deve ter 14 dígitos.")] // CNPJ sem mascara possui 14 digitos numericos
    public string Document { get; set; } = string.Empty; // Inicializa com string vazia para seguranca contra null

    /// <summary>
    /// Nome completo do administrador responsavel pela conta da empresa.
    /// Campo obrigatorio com limite de 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome do administrador é obrigatório.")] // Garante que o nome do administrador seja informado
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")] // Restricao de tamanho para o campo de nome
    public string AdminName { get; set; } = string.Empty; // Valor default como string vazia

    /// <summary>
    /// Endereco de e-mail do administrador, utilizado para login e comunicacoes.
    /// Deve estar em formato de e-mail valido (ex: usuario@dominio.com).
    /// </summary>
    [Required(ErrorMessage = "O e-mail é obrigatório.")] // Campo obrigatorio para identificacao do usuario
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")] // Valida se o texto informado segue o padrao de endereco de e-mail
    [MaxLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")] // Tamanho maximo conforme boas praticas de Identity
    public string Email { get; set; } = string.Empty; // Inicializa com string vazia por seguranca

    /// <summary>
    /// Senha de acesso do administrador.
    /// Deve conter no minimo 8 caracteres para atender requisitos minimos de seguranca.
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória.")] // A senha e obrigatoria para autenticacao
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")] // Define o comprimento minimo da senha para seguranca basica
    public string Password { get; set; } = string.Empty; // Inicializa com string vazia por seguranca contra null
}
