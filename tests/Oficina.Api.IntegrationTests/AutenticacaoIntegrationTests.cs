using System.Net;
using System.Net.Http.Headers;
using Oficina.Api.IntegrationTests.Support;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Auth;

namespace Oficina.Api.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class AutenticacaoIntegrationTests : IntegrationTestBase
{
    public AutenticacaoIntegrationTests(OficinaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaTokenJwt()
    {
        var client = ClienteAnonimo();

        var resposta = await client.PostJsonAsync("/Auth/login", new LoginInputDto
        {
            UserName = OficinaApiFactory.AdminUserName,
            Senha = OficinaApiFactory.AdminSenha
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.OK);
        resposta.DeveSerJson();

        var login = await resposta.LerAsync<LoginResultDto>();

        // Um JWT tem três partes separadas por ponto: cabeçalho, conteúdo e assinatura
        Assert.Equal(3, login.Token.Split('.').Length);
        Assert.True(login.ExpiraEm > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var client = ClienteAnonimo();

        var resposta = await client.PostJsonAsync("/Auth/login", new LoginInputDto
        {
            UserName = OficinaApiFactory.AdminUserName,
            Senha = "senha-errada"
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComUsuarioInexistente_Retorna401()
    {
        var client = ClienteAnonimo();

        var resposta = await client.PostJsonAsync("/Auth/login", new LoginInputDto
        {
            UserName = DadosDeTeste.Texto("inexistente"),
            Senha = "Qualquer@123"
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_SemInformarSenha_Retorna400()
    {
        var client = ClienteAnonimo();

        var resposta = await client.PostJsonAsync("/Auth/login", new { userName = OficinaApiFactory.AdminUserName });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/Clientes")]
    [InlineData("/Veiculos")]
    [InlineData("/Servicos")]
    [InlineData("/Catalogo")]
    [InlineData("/Estoques")]
    [InlineData("/OrdensServico")]
    [InlineData("/Indicadores/tempo-medio-execucao")]
    public async Task EndpointAdministrativo_SemToken_Retorna401(string rota)
    {
        var client = ClienteAnonimo();

        var resposta = await client.GetAsync(rota);

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndpointAdministrativo_ComTokenInvalido_Retorna401()
    {
        var client = ClienteAnonimo();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token.invalido.qualquer");

        var resposta = await client.GetAsync("/Clientes");

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndpointAdministrativo_ComTokenValido_Retorna200()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.GetAsync("/Clientes");

        await resposta.DeveTerStatusAsync(HttpStatusCode.OK);
        resposta.DeveSerJson();
    }

    [Fact]
    public async Task Mecanico_ConsultaCadastros_Retorna200()
    {
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.GetAsync("/Clientes");

        await resposta.DeveTerStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Mecanico_TentaCadastrarCliente_Retorna403()
    {
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.PostJsonAsync("/Clientes", new ClienteInputDto
        {
            Nome = "Cliente Proibido",
            Documento = DadosDeTeste.Cpf()
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Mecanico_TentaConsultarIndicadores_Retorna403()
    {
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.GetAsync("/Indicadores/tempo-medio-execucao");

        await resposta.DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }
}
