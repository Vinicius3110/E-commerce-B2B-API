// Namespace para exceções customizadas do domínio
namespace EcommerceB2B.Domain.Exceptions;

// Classe base para todas as exceções de regra de negócio do domínio
// Herda de Exception para ser compatível com o sistema de exceções do .NET
// Usar uma exceção customizada permite diferenciar erros de negócio de erros técnicos
public class DomainException : Exception
{
    // Construtor padrão: recebe apenas a mensagem de erro
    // base(message) chama o construtor da classe pai (Exception) passando a mensagem
    public DomainException(string message) : base(message)
    {
    }

    // Construtor com mensagem e exceção interna (para encadeamento de exceções)
    // Útil quando uma exceção é causada por outra e queremos preservar a causa original
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
