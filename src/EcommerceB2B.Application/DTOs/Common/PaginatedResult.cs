// Define o namespace dentro da camada de Aplicacao, na area de DTOs comuns/compartilhados
namespace EcommerceB2B.Application.DTOs.Common;

/// <summary>
/// DTO generico para representar resultados paginados em qualquer listagem da API.
/// Encapsula os itens da pagina atual junto com metadados de paginacao para o cliente.
/// </summary>
/// <typeparam name="T">Tipo dos itens contidos na pagina (ex: ProductDto, OrderDto, etc.)</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Lista somente-leitura dos itens da pagina atual.
    /// IReadOnlyList impede modificacoes acidentais na colecao apos a construcao do resultado.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>(); // Itens da pagina atual (inicializado como array vazio)

    /// <summary>
    /// Numero da pagina atual (baseado em 1).
    /// Ex: pagina 1, pagina 2, etc. Informado pelo cliente na requisicao.
    /// </summary>
    public int Page { get; set; } // Numero da pagina atual

    /// <summary>
    /// Quantidade de itens por pagina.
    /// Definido pelo cliente na requisicao ou usa valor padrao do servidor.
    /// </summary>
    public int PageSize { get; set; } // Tamanho da pagina (itens por pagina)

    /// <summary>
    /// Quantidade total de registros encontrados (em todas as paginas).
    /// Utilizado pelo cliente para calcular o numero total de paginas e exibir "Mostrando X de Y resultados".
    /// </summary>
    public int TotalCount { get; set; } // Total de registros existentes

    /// <summary>
    /// Numero total de paginas disponiveis.
    /// Propriedade computada: arredonda para cima a divisao de TotalCount por PageSize.
    /// Ex: se TotalCount=95 e PageSize=10, TotalPages=10 (9 paginas cheias + 1 com 5 itens).
    /// </summary>
    public int TotalPages => (int)System.Math.Ceiling(TotalCount / (double)PageSize); // Calcula o total de paginas (arredondamento para cima)

    /// <summary>
    /// Indica se existe uma pagina anterior disponivel.
    /// Verdadeiro quando Page > 1, falso quando ja esta na primeira pagina.
    /// </summary>
    public bool HasPreviousPage => Page > 1; // Ha pagina anterior se a pagina atual for maior que 1

    /// <summary>
    /// Indica se existe uma proxima pagina disponivel.
    /// Verdadeiro quando Page < TotalPages, falso quando ja esta na ultima pagina.
    /// </summary>
    public bool HasNextPage => Page < TotalPages; // Ha proxima pagina se a pagina atual for menor que o total de paginas
}
