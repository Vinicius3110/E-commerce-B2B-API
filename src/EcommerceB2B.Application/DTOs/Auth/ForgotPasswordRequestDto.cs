// Importa o namespace para utilizar atributos de validacao (Data Annotations)
using System.ComponentModel.DataAnnotations;

// Define o namespace do DTO dentro da camada de Aplicacao, na area de Autenticacao
namespace EcommerceB2B.Application.DTOs.Auth;

/// <summary>
/// DTO utilizado quando o usuario solicita a redefinicao de senha (esqueci minha senha).
/// A API enviara um e-mail com instrucoes e um token de redefinicao para o endereco informado.
/// </summary>
public class ForgotPasswordRequestDto
{
    /// <summary>
    /// Endereco de e-mail da conta para a qual se deseja redefinir a senha.
    /// Deve ser um e-mail valido ja cadastrado no sistema.
    /// </summary>
    [Required(ErrorMessage = "O e-mail é obrigatório.")] // Campo obrigatorio para identificar a conta do usuario
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")] // Valida o formato do e-mail antes de processar a solicitacao
    public string Email { get; set; } = string.Empty; // Inicializa com string vazia para seguranca contra null
}
