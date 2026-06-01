// Importa a interface de logging para registrar as operações simuladas de email
using Microsoft.Extensions.Logging;

// Namespace que organiza as classes relacionadas à autenticação
namespace EcommerceB2B.Infrastructure.Auth;

// Serviço de envio de emails — implementação simulada para fins de estudo
// Em produção, esta classe seria substituída por uma implementação real
// que usa SMTP, SendGrid, AWS SES ou outro provedor de email
// Registrado como Scoped no contêiner DI
public class EmailService
{
    // Logger injetado para registrar as operações de email no console/arquivo
    // ILogger<EmailService> é o logger tipado que categoriza as mensagens com o nome da classe
    // readonly garante que a referência não será alterada após a construção
    private readonly ILogger<EmailService> _logger;

    // Construtor que recebe o logger por injeção de dependência
    // O contêiner DI do ASP.NET Core resolve e injeta o logger automaticamente
    public EmailService(ILogger<EmailService> logger)
    {
        // Armazena a referência do logger para uso nos métodos da classe
        _logger = logger;
    }

    // Envia email de confirmação de cadastro para o usuário
    // No ambiente real, este método enviaria um email com um link de confirmação
    // Parâmetros:
    //   email: endereço de email do destinatário
    //   confirmationLink: link que o usuário deve clicar para confirmar o email
    //                      gerado pelo ASP.NET Core Identity via GenerateEmailConfirmationTokenAsync
    public Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // Loga o link de confirmação no console — simulando o envio de email
        // Em produção, aqui seria usado SmtpClient, HttpClient para API de email, etc.
        // O formato [EMAIL SIMULADO] deixa claro nos logs que é uma simulação
        _logger.LogInformation(
            "[EMAIL SIMULADO] Confirmação de email para: {Email}",
            email);

        // Loga o link de confirmação em uma linha separada para facilitar a leitura
        _logger.LogInformation(
            "[EMAIL SIMULADO] Link de confirmação: {Link}",
            confirmationLink);

        // Retorna Task.CompletedTask pois não há operação assíncrona real
        // Em uma implementação real, seria await smtpClient.SendMailAsync(...)
        return Task.CompletedTask;
    }

    // Envia email de redefinição de senha para o usuário
    // No ambiente real, este método enviaria um email com um link seguro
    // Parâmetros:
    //   email: endereço de email do destinatário
    //   resetLink: link para a página de redefinição de senha
    //              gerado pelo ASP.NET Core Identity via GeneratePasswordResetTokenAsync
    public Task SendPasswordResetAsync(string email, string resetLink)
    {
        // Loga o link de redefinição no console — simulando o envio de email
        // Em produção, aqui seria implementado o envio real via SMTP ou API
        _logger.LogInformation(
            "[EMAIL SIMULADO] Redefinição de senha para: {Email}",
            email);

        // Loga o link de redefinição para que o desenvolvedor possa copiá-lo
        // O link contém o token de redefinição e o email do usuário como parâmetros
        _logger.LogInformation(
            "[EMAIL SIMULADO] Link de redefinição: {Link}",
            resetLink);

        // Retorna Task.CompletedTask — operação simulada concluída
        return Task.CompletedTask;
    }
}
