using System.Net;
using System.Net.Http.Headers;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Auth;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.DTOs.Usuario;
using Oficina.Domain.Enums;

namespace Oficina.Api.IntegrationTests.Support;

/// <summary>
/// Funções comuns dos testes: clientes HTTP (anônimo, administrador e mecânico) e cadastros auxiliares
/// feitos pela própria API, cada um com dados únicos.
/// </summary>
public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(OficinaApiFactory factory)
    {
        Factory = factory;
    }

    protected OficinaApiFactory Factory { get; }

    protected HttpClient ClienteAnonimo() => Factory.CriarClienteHttp();

    protected Task<HttpClient> ClienteAdminAsync()
        => ClienteAutenticadoAsync(OficinaApiFactory.AdminUserName, OficinaApiFactory.AdminSenha);

    protected async Task<HttpClient> ClienteMecanicoAsync()
    {
        var admin = await ClienteAdminAsync();
        var userName = DadosDeTeste.Texto("mecanico");
        const string senha = "Mec@nico123";

        await (await admin.PostJsonAsync("/Usuarios", new UsuarioInputDto
        {
            Nome = "Mecânico de Integração",
            UserName = userName,
            Senha = senha,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await ClienteAutenticadoAsync(userName, senha);
    }

    protected async Task<HttpClient> ClienteAutenticadoAsync(string userName, string senha)
    {
        var client = Factory.CriarClienteHttp();

        var resposta = await (await client.PostJsonAsync("/Auth/login", new LoginInputDto
        {
            UserName = userName,
            Senha = senha
        })).DeveTerStatusAsync(HttpStatusCode.OK);

        var login = await resposta.LerAsync<LoginResultDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        return client;
    }

    protected static async Task<ClienteDto> CriarClienteAsync(HttpClient admin, string? documento = null)
    {
        var resposta = await (await admin.PostJsonAsync("/Clientes", new ClienteInputDto
        {
            Nome = DadosDeTeste.Texto("Cliente"),
            Documento = documento ?? DadosDeTeste.Cpf()
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<ClienteDto>();
    }

    protected static async Task<VeiculoDto> CriarVeiculoAsync(HttpClient admin, int clienteId, string? placa = null)
    {
        var resposta = await (await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = clienteId,
            Placa = placa ?? DadosDeTeste.Placa(),
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<VeiculoDto>();
    }

    protected static async Task<ServicoDto> CriarServicoAsync(HttpClient admin, decimal valor, bool ativo = true)
    {
        var resposta = await (await admin.PostJsonAsync("/Servicos", new ServicoInputDto
        {
            Descricao = DadosDeTeste.Texto("Servico"),
            Valor = valor,
            Ativo = ativo
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<ServicoDto>();
    }

    protected static async Task<CatalogoDto> CriarCatalogoAsync(
        HttpClient admin,
        decimal preco,
        EnumsConsumivel.TipoConsumivel tipo = EnumsConsumivel.TipoConsumivel.Peca,
        bool ativo = true)
    {
        var resposta = await (await admin.PostJsonAsync("/Catalogo", new CatalogoInputDto
        {
            Nome = DadosDeTeste.Texto("Item"),
            Preco = preco,
            Tipo = tipo,
            Ativo = ativo,
            Descricao = "Criado pelo teste de integração"
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<CatalogoDto>();
    }

    protected static async Task<EstoqueDto> CriarEstoqueAsync(HttpClient admin, int catalogoId, int quantidade)
    {
        var resposta = await (await admin.PostJsonAsync("/Estoques", new EstoqueInputDto
        {
            CatalogoId = catalogoId,
            Quantidade = quantidade
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<EstoqueDto>();
    }

    protected static async Task<(CatalogoDto Catalogo, EstoqueDto Estoque)> CriarPecaComEstoqueAsync(
        HttpClient admin,
        decimal preco,
        int quantidade)
    {
        var catalogo = await CriarCatalogoAsync(admin, preco);
        var estoque = await CriarEstoqueAsync(admin, catalogo.Id, quantidade);

        return (catalogo, estoque);
    }
}
