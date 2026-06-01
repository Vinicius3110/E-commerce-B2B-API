// Namespace que agrupa os tipos enumerados do domínio (enums)
// Enums são tipos de valor que representam um conjunto fixo de opções nomeadas
namespace EcommerceB2B.Domain.Enums;

// Define os tipos possíveis de empresa no sistema B2B
// O enum é do tipo byte para economizar espaço no banco (0-255 valores possíveis)
public enum CompanyType : byte
{
    // Empresa que apenas compra produtos de outras empresas
    Comprador = 1,

    // Empresa que apenas vende produtos para outras empresas
    Vendedor = 2,

    // Empresa que tanto compra quanto vende produtos na plataforma
    Ambos = 3
}
