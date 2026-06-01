// Importa os DTOs de empresa definidos na camada de aplicacao
using EcommerceB2B.Application.DTOs.Company;

// Importa a entidade Company do dominio
using EcommerceB2B.Domain.Entities;

// Alias para evitar conflito entre o nome da entidade Company e o namespace Company
// O compilador confundiria Company (entidade) com Company (namespace deste arquivo)
using CompanyEntity = EcommerceB2B.Domain.Entities.Company;

// Importa os tipos enumerados do dominio (CompanyType)
using EcommerceB2B.Domain.Enums;

// Importa a excecao customizada de dominio para erros de regra de negocio
using EcommerceB2B.Domain.Exceptions;

// Importa a interface de repositorio de empresas definida no dominio
using EcommerceB2B.Domain.Interfaces;

// Importa o ASP.NET Core Identity para gerenciamento de usuarios
using Microsoft.AspNetCore.Identity;

// Importa a interface de logging para registrar eventos e erros
using Microsoft.Extensions.Logging;

// Namespace que agrupa os servicos de empresa na camada de aplicacao
namespace EcommerceB2B.Application.UseCases.Company;

// Servico de empresa: orquestra operacoes relacionadas a empresas (tenants)
// Gerencia dados da empresa, usuarios vinculados e operacoes de CRUD
// Depende do repositorio de empresas e do UserManager do Identity
public class CompanyService
{
    // Repositorio de empresas (definido no dominio, implementado no Infrastructure)
    // Fornece acesso a dados de empresas: busca por ID, documento, criacao e atualizacao
    private readonly ICompanyRepository _companyRepository;

    // Gerenciador de usuarios do ASP.NET Core Identity
    // Usado para criar usuarios vinculados a uma empresa
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    // Logger tipado para a classe CompanyService
    // Usado para registrar operacoes de CRUD e erros
    private readonly ILogger<CompanyService> _logger;

    // Construtor que recebe as dependencias por injecao de dependencia
    // O contêiner DI do ASP.NET Core resolve e injeta cada parametro
    public CompanyService(
        ICompanyRepository companyRepository,
        UserManager<IdentityUser<Guid>> userManager,
        ILogger<CompanyService> logger)
    {
        // Armazena as dependencias nos campos readonly
        _companyRepository = companyRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // Caso de uso: Obter empresa por ID
    // Busca a empresa no repositorio e converte para DTO
    // Se a empresa nao existir, lanca DomainException
    public async Task<CompanyDto> GetByIdAsync(Guid companyId)
    {
        // Registra a operacao no log
        _logger.LogInformation("Buscando empresa por ID: {CompanyId}", companyId);

        // Busca a empresa no repositorio
        // GetByIdAsync retorna null se a empresa nao for encontrada
        var company = await _companyRepository.GetByIdAsync(companyId);

        // Se a empresa nao existir, lanca excecao de dominio
        if (company is null)
        {
            throw new DomainException("Empresa não encontrada.");
        }

        // Converte a entidade Company para CompanyDto e retorna
        return MapToDto(company);
    }

    // Caso de uso: Atualizar dados de uma empresa
    // Verifica se a empresa existe, aplica as alteracoes e persiste
    // Valida o tipo da empresa (Comprador, Vendedor, Ambos)
    public async Task<CompanyDto> UpdateAsync(Guid companyId, UpdateCompanyDto request)
    {
        // Registra a operacao de atualizacao no log
        _logger.LogInformation("Atualizando empresa: {CompanyId}", companyId);

        // Busca a empresa pelo ID
        var company = await _companyRepository.GetByIdAsync(companyId);

        // Se a empresa nao existir, lanca excecao
        if (company is null)
        {
            throw new DomainException("Empresa não encontrada.");
        }

        // Atualiza o nome da empresa usando o metodo SetName da entidade
        // SetName valida se o nome nao e vazio ou apenas espacos
        company.SetName(request.Name);

        // Converte a string de tipo para o enum CompanyType
        // Enum.TryParse tenta converter a string para o valor do enum
        // O parametro ignoreCase: true permite "comprador", "Comprador", etc.
        if (!Enum.TryParse<CompanyType>(request.Type, ignoreCase: true, out var companyType))
        {
            // Se a string nao corresponder a nenhum valor do enum, lanca excecao
            throw new DomainException(
                "Tipo de empresa inválido. Valores válidos: Comprador, Vendedor, Ambos.");
        }

        // Atualiza o tipo da empresa usando o metodo SetType da entidade
        // SetType valida se o tipo e um valor valido do enum CompanyType
        company.SetType(companyType);

        // Persiste as alteracoes no banco via repositorio
        // Update marca a entidade como modificada no ChangeTracker do EF Core
        _companyRepository.Update(company);

        // Registra sucesso no log
        _logger.LogInformation("Empresa {CompanyId} atualizada com sucesso", companyId);

        // Converte para DTO e retorna os dados atualizados
        return MapToDto(company);
    }

    // Caso de uso: Criar um novo usuario vinculado a uma empresa
    // Verifica se a empresa existe, cria o IdentityUser e atribui a role
    // Retorna os dados do usuario criado como UserDto
    public async Task<UserDto> CreateUserAsync(Guid companyId, CreateUserDto request)
    {
        // Registra a operacao no log
        _logger.LogInformation(
            "Criando usuario {Email} para empresa: {CompanyId}",
            request.Email,
            companyId);

        // Verifica se a empresa existe no banco
        var company = await _companyRepository.GetByIdAsync(companyId);

        // Se a empresa nao existir, nao faz sentido criar usuario
        if (company is null)
        {
            throw new DomainException("Empresa não encontrada.");
        }

        // Cria o IdentityUser com os dados do DTO
        var user = new IdentityUser<Guid>
        {
            // UserName e obrigatorio — usamos o email como nome de usuario
            UserName = request.Email,

            // Email para contato e login
            Email = request.Email,

            // EmailConfirmed = true: usuarios criados por admin nao precisam confirmar email
            // Isso e diferente do auto-registro onde o email precisa ser confirmado
            EmailConfirmed = true
        };

        // Cria o usuario no Identity com a senha fornecida
        var createResult = await _userManager.CreateAsync(user, request.Password);

        // Se a criacao falhar (senha fraca, email duplicado, etc.), lanca excecao
        if (!createResult.Succeeded)
        {
            // Concatena todos os erros de validacao do Identity
            var errors = string.Join("; ",
                createResult.Errors.Select(e => e.Description));

            // Lanca DomainException com os detalhes
            throw new DomainException($"Falha ao criar usuário: {errors}");
        }

        // Atribui a role especificada ao usuario
        // AddToRoleAsync vincula o usuario ao perfil (Admin, Comprador, Vendedor)
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

        // Se a atribuicao de role falhar, lanca excecao
        if (!roleResult.Succeeded)
        {
            // Concatena os erros e lanca DomainException
            var errors = string.Join("; ",
                roleResult.Errors.Select(e => e.Description));
            throw new DomainException($"Falha ao atribuir perfil: {errors}");
        }

        // Registra sucesso no log
        _logger.LogInformation(
            "Usuario {UserId} criado para empresa {CompanyId} com role {Role}",
            user.Id,
            companyId,
            request.Role);

        // Converte o IdentityUser para UserDto e retorna
        return new UserDto
        {
            Id = user.Id,
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            IsActive = true
        };
    }

    // Caso de uso: Listar usuarios de uma empresa
    // NOTA: Implementacao pendente — retorna lista vazia
    // Necessita de CompanyUserRepository para consultar o vinculo usuario-empresa
    // Sera implementado quando o repositorio de CompanyUser estiver disponivel
    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(Guid companyId)
    {
        // Registra a operacao no log
        _logger.LogInformation(
            "Listando usuarios da empresa: {CompanyId} (implementacao pendente)",
            companyId);

        // Verifica se a empresa existe
        var company = await _companyRepository.GetByIdAsync(companyId);

        // Se a empresa nao existir, lanca excecao
        if (company is null)
        {
            throw new DomainException("Empresa não encontrada.");
        }

        // Retorna lista vazia — implementacao pendente do CompanyUserRepository
        // Futuramente, buscara os CompanyUsers vinculados a esta empresa
        // e mapeara os IdentityUsers para UserDto
        return Array.Empty<UserDto>();
    }

    // Metodo auxiliar privado: converte entidade Company para CompanyDto
    // Centraliza o mapeamento para evitar duplicacao de codigo nos metodos publicos
    // O DTO expoe apenas os dados necessarios, ocultando detalhes internos da entidade
    private static CompanyDto MapToDto(CompanyEntity company)
    {
        // Cria e retorna o DTO preenchido com os dados da entidade
        return new CompanyDto
        {
            // Copia o identificador unico
            Id = company.Id,

            // Copia o nome (razao social ou nome fantasia)
            Name = company.Name,

            // Copia o documento (CNPJ)
            Document = company.Document,

            // Converte o enum CompanyType para string usando ToString()
            // Ex: CompanyType.Comprador → "Comprador"
            Type = company.Type.ToString(),

            // Copia o status de atividade
            IsActive = company.IsActive,

            // Copia a data de criacao
            CreatedAt = company.CreatedAt
        };
    }
}
