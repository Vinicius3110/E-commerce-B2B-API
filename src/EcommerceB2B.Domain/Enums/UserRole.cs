// Namespace que agrupa os tipos enumerados do domínio
namespace EcommerceB2B.Domain.Enums;

// Define os papéis (roles) que um usuário pode ter dentro de uma empresa
// Estes valores são usados tanto no Identity quanto nas regras de autorização
public enum UserRole : byte
{
    // Administrador da empresa — pode gerenciar usuários, produtos e pedidos
    Admin = 1,

    // Usuário com permissão para comprar (visualizar produtos, criar pedidos)
    Comprador = 2,

    // Usuário com permissão para vender (anunciar produtos, gerenciar pedidos recebidos)
    Vendedor = 3
}
