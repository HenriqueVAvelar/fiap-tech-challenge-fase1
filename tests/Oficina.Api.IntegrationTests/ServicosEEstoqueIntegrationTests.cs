using System.Net;
using Oficina.Api.IntegrationTests.Support;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Domain.Enums;

namespace Oficina.Api.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class ServicosEEstoqueIntegrationTests : IntegrationTestBase
{
    public ServicosEEstoqueIntegrationTests(OficinaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Servico_CicloCompletoDoCrud()
    {
        var admin = await ClienteAdminAsync();

        var criacao = await admin.PostJsonAsync("/Servicos", new ServicoInputDto
        {
            Descricao = "Troca de óleo",
            Valor = 120.50m,
            Ativo = true
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();

        var criado = await criacao.LerAsync<ServicoDto>();
        Assert.True(criado.Id > 0);
        Assert.Equal("Troca de óleo", criado.Descricao);
        Assert.Equal(120.50m, criado.Valor);
        Assert.True(criado.Ativo);

        var consulta = await (await admin.GetAsync($"/Servicos/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal("Troca de óleo", (await consulta.LerAsync<ServicoDto>()).Descricao);

        var lista = await (await admin.GetAsync("/Servicos")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Contains(await lista.LerAsync<List<ServicoDto>>(), s => s.Id == criado.Id);

        await (await admin.PutJsonAsync($"/Servicos/{criado.Id}", new ServicoInputDto
        {
            Descricao = "Alinhamento",
            Valor = 90m,
            Ativo = false
        })).DeveTerStatusAsync(HttpStatusCode.NoContent);

        var apos = await (await admin.GetAsync($"/Servicos/{criado.Id}")).LerAsync<ServicoDto>();
        Assert.Equal("Alinhamento", apos.Descricao);
        Assert.Equal(90m, apos.Valor);
        Assert.False(apos.Ativo);

        await (await admin.DeleteAsync($"/Servicos/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NoContent);
        await (await admin.GetAsync($"/Servicos/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Servico_ComValorNegativo_Retorna400()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/Servicos", new ServicoInputDto
        {
            Descricao = "Valor inválido",
            Valor = -1m,
            Ativo = true
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Catalogo_CicloCompletoDoCrud()
    {
        var admin = await ClienteAdminAsync();

        var criacao = await admin.PostJsonAsync("/Catalogo", new CatalogoInputDto
        {
            Nome = DadosDeTeste.Texto("Filtro"),
            Preco = 35m,
            Tipo = EnumsConsumivel.TipoConsumivel.Peca,
            Ativo = true,
            Descricao = "Filtro de óleo"
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();

        var criado = await criacao.LerAsync<CatalogoDto>();
        Assert.Equal(35m, criado.Preco);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, criado.Tipo);

        await (await admin.GetAsync($"/Catalogo/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.OK);

        await (await admin.PutJsonAsync($"/Catalogo/{criado.Id}", new CatalogoInputDto
        {
            Nome = criado.Nome,
            Preco = 42m,
            Tipo = EnumsConsumivel.TipoConsumivel.Insumo,
            Ativo = true,
            Descricao = "Reclassificado"
        })).DeveTerStatusAsync(HttpStatusCode.NoContent);

        var apos = await (await admin.GetAsync($"/Catalogo/{criado.Id}")).LerAsync<CatalogoDto>();
        Assert.Equal(42m, apos.Preco);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Insumo, apos.Tipo);

        await (await admin.DeleteAsync($"/Catalogo/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NoContent);
        await (await admin.GetAsync($"/Catalogo/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Estoque_ControlaEntradasEBaixas()
    {
        var admin = await ClienteAdminAsync();
        var catalogo = await CriarCatalogoAsync(admin, 20m, EnumsConsumivel.TipoConsumivel.Insumo);

        var criacao = await admin.PostJsonAsync("/Estoques", new EstoqueInputDto
        {
            CatalogoId = catalogo.Id,
            Quantidade = 5
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();

        var estoque = await criacao.LerAsync<EstoqueDto>();
        Assert.Equal(catalogo.Id, estoque.CatalogoId);
        Assert.Equal(5, estoque.Quantidade);

        // Entrada: 5 + 3
        var entrada = await admin.PostJsonAsync($"/Estoques/{estoque.Id}/entrada", new EstoqueMovimentacaoInputDto { Quantidade = 3 });
        await entrada.DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(8, (await entrada.LerAsync<EstoqueDto>()).Quantidade);

        // Baixa: 8 - 2
        var baixa = await admin.PostJsonAsync($"/Estoques/{estoque.Id}/baixa", new EstoqueMovimentacaoInputDto { Quantidade = 2 });
        await baixa.DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(6, (await baixa.LerAsync<EstoqueDto>()).Quantidade);

        // O saldo fica gravado
        var consulta = await (await admin.GetAsync($"/Estoques/{estoque.Id}")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(6, (await consulta.LerAsync<EstoqueDto>()).Quantidade);
    }

    [Fact]
    public async Task Estoque_BaixaMaiorQueOSaldo_Retorna409ESaldoNaoMuda()
    {
        var admin = await ClienteAdminAsync();
        var (_, estoque) = await CriarPecaComEstoqueAsync(admin, 10m, 4);

        var resposta = await admin.PostJsonAsync($"/Estoques/{estoque.Id}/baixa", new EstoqueMovimentacaoInputDto { Quantidade = 5 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);

        var consulta = await (await admin.GetAsync($"/Estoques/{estoque.Id}")).LerAsync<EstoqueDto>();
        Assert.Equal(4, consulta.Quantidade);
    }

    [Fact]
    public async Task Estoque_MovimentacaoComQuantidadeZero_Retorna400()
    {
        var admin = await ClienteAdminAsync();
        var (_, estoque) = await CriarPecaComEstoqueAsync(admin, 10m, 4);

        var resposta = await admin.PostJsonAsync($"/Estoques/{estoque.Id}/entrada", new EstoqueMovimentacaoInputDto { Quantidade = 0 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Estoque_DeItemDeCatalogoInexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/Estoques", new EstoqueInputDto { CatalogoId = int.MaxValue, Quantidade = 1 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Estoque_AtualizarEExcluir()
    {
        var admin = await ClienteAdminAsync();
        var (_, estoque) = await CriarPecaComEstoqueAsync(admin, 10m, 4);

        await (await admin.PutJsonAsync($"/Estoques/{estoque.Id}", new EstoqueUpdateInputDto { Quantidade = 20 }))
            .DeveTerStatusAsync(HttpStatusCode.NoContent);

        Assert.Equal(20, (await (await admin.GetAsync($"/Estoques/{estoque.Id}")).LerAsync<EstoqueDto>()).Quantidade);

        await (await admin.DeleteAsync($"/Estoques/{estoque.Id}")).DeveTerStatusAsync(HttpStatusCode.NoContent);
        await (await admin.GetAsync($"/Estoques/{estoque.Id}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Estoque_Mecanico_PodeDarBaixaMasNaoRegistrarEntrada()
    {
        var admin = await ClienteAdminAsync();
        var mecanico = await ClienteMecanicoAsync();
        var (_, estoque) = await CriarPecaComEstoqueAsync(admin, 10m, 4);

        await (await mecanico.PostJsonAsync($"/Estoques/{estoque.Id}/baixa", new EstoqueMovimentacaoInputDto { Quantidade = 1 }))
            .DeveTerStatusAsync(HttpStatusCode.OK);

        await (await mecanico.PostJsonAsync($"/Estoques/{estoque.Id}/entrada", new EstoqueMovimentacaoInputDto { Quantidade = 1 }))
            .DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }
}
