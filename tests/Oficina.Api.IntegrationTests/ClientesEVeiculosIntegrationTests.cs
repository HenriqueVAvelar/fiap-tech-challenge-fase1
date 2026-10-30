using System.Net;
using Oficina.Api.IntegrationTests.Support;
using Oficina.Application.DTOs;
using Oficina.Domain.Enums;

namespace Oficina.Api.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class ClientesEVeiculosIntegrationTests : IntegrationTestBase
{
    public ClientesEVeiculosIntegrationTests(OficinaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Cliente_CicloCompletoDoCrud()
    {
        var admin = await ClienteAdminAsync();
        var cpf = DadosDeTeste.Cpf();

        // Cria: o CPF informado com máscara é guardado apenas com os dígitos
        var criacao = await admin.PostJsonAsync("/Clientes", new ClienteInputDto
        {
            Nome = "Maria da Silva",
            Documento = DadosDeTeste.FormatarCpf(cpf)
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();
        Assert.EndsWith($"/Clientes/{cpf}", criacao.Headers.Location?.ToString());

        var criado = await criacao.LerAsync<ClienteDto>();
        Assert.True(criado.Id > 0);
        Assert.Equal("Maria da Silva", criado.Nome);
        Assert.Equal(cpf, criado.Documento);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cpf, criado.TipoDocumento);

        // Consulta pelo documento
        var consulta = await admin.GetAsync($"/Clientes/{cpf}");
        await consulta.DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(criado.Id, (await consulta.LerAsync<ClienteDto>()).Id);

        // Lista
        var lista = await (await admin.GetAsync("/Clientes")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Contains(await lista.LerAsync<List<ClienteDto>>(), c => c.Documento == cpf);

        // Atualiza
        var atualizacao = await admin.PutJsonAsync($"/Clientes/{cpf}", new ClienteInputDto
        {
            Nome = "Maria de Souza",
            Documento = cpf
        });
        await atualizacao.DeveTerStatusAsync(HttpStatusCode.NoContent);

        var apos = await (await admin.GetAsync($"/Clientes/{cpf}")).LerAsync<ClienteDto>();
        Assert.Equal("Maria de Souza", apos.Nome);

        // Remove
        await (await admin.DeleteAsync($"/Clientes/{cpf}")).DeveTerStatusAsync(HttpStatusCode.NoContent);
        await (await admin.GetAsync($"/Clientes/{cpf}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cliente_ComCnpjValido_Retorna201()
    {
        var admin = await ClienteAdminAsync();
        var cnpj = DadosDeTeste.Cnpj();

        var cliente = await CriarClienteAsync(admin, cnpj);

        Assert.Equal(cnpj, cliente.Documento);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cnpj, cliente.TipoDocumento);
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("12345")]
    [InlineData("11.111.111/1111-11")]
    public async Task Cliente_ComDocumentoInvalido_Retorna400(string documento)
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/Clientes", new ClienteInputDto
        {
            Nome = "Documento Inválido",
            Documento = documento
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cliente_ComDocumentoJaCadastrado_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await admin.PostJsonAsync("/Clientes", new ClienteInputDto
        {
            Nome = "Outro Nome",
            Documento = DadosDeTeste.FormatarCpf(cliente.Documento)
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cliente_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.GetAsync($"/Clientes/{DadosDeTeste.Cpf()}");

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Veiculo_CicloCompletoDoCrud()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var placa = DadosDeTeste.Placa();

        // Cria: a placa digitada em minúsculas e com hífen é normalizada
        var criacao = await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = $"{placa[..3]}-{placa[3..]}".ToLowerInvariant(),
            Marca = "Volkswagen",
            Modelo = "Gol",
            Ano = 2019
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();

        var criado = await criacao.LerAsync<VeiculoDto>();
        Assert.Equal(placa, criado.Placa);
        Assert.Equal(cliente.Id, criado.ClienteId);
        Assert.Equal("Volkswagen", criado.Marca);
        Assert.Equal("Gol", criado.Modelo);
        Assert.Equal(2019, criado.Ano);

        // Consulta pela placa
        var consulta = await (await admin.GetAsync($"/Veiculos/{placa}")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(criado.Id, (await consulta.LerAsync<VeiculoDto>()).Id);

        // Atualiza
        var atualizacao = await admin.PutJsonAsync($"/Veiculos/{placa}", new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = placa,
            Marca = "Volkswagen",
            Modelo = "Polo",
            Ano = 2022
        });
        await atualizacao.DeveTerStatusAsync(HttpStatusCode.NoContent);

        var apos = await (await admin.GetAsync($"/Veiculos/{placa}")).LerAsync<VeiculoDto>();
        Assert.Equal("Polo", apos.Modelo);
        Assert.Equal(2022, apos.Ano);

        // Remove
        await (await admin.DeleteAsync($"/Veiculos/{placa}")).DeveTerStatusAsync(HttpStatusCode.NoContent);
        await (await admin.GetAsync($"/Veiculos/{placa}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Veiculo_ComPlacaNoPadraoAntigo_Retorna201()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var placa = $"ZZZ{Random.Shared.Next(1000, 9999)}";

        var veiculo = await CriarVeiculoAsync(admin, cliente.Id, placa);

        Assert.Equal(placa, veiculo.Placa);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("1234567")]
    [InlineData("AB1D234")]
    public async Task Veiculo_ComPlacaInvalida_Retorna400(string placa)
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = placa,
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Veiculo_ComPlacaJaCadastrada_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);

        var resposta = await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = veiculo.Placa,
            Marca = "Fiat",
            Modelo = "Palio",
            Ano = 2018
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Veiculo_DeClienteInexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = int.MaxValue,
            Placa = DadosDeTeste.Placa(),
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Veiculo_ComAnoForaDoIntervalo_Retorna400()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await admin.PostJsonAsync("/Veiculos", new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = DadosDeTeste.Placa(),
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 1800
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }
}
