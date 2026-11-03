using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Catalogo;

public class CatalogoServiceConsultaTests
{
    private readonly FakeCatalogoRepository _repository = new();
    private readonly CatalogoService _service;

    public CatalogoServiceConsultaTests()
    {
        _service = new CatalogoService(_repository, CadastroFakes.CriarMapper());
    }

    private static CatalogoInputDto Input(string nome = "Óleo 5W30", decimal preco = 30m)
        => new()
        {
            Nome = nome,
            Preco = preco,
            Tipo = EnumsConsumivel.TipoConsumivel.Insumo,
            Ativo = true,
            Descricao = "Sintético"
        };

    [Fact]
    public async Task ObterPorId_DeveRetornarNull_QuandoNaoExiste()
    {
        Assert.Null(await _service.ObterPorIdAsync(42));
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarListaVazia_QuandoNaoHaItens()
    {
        Assert.Empty(await _service.ObterTodosAsync());
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoDadosInvalidos_ENaoPersistir()
    {
        var criado = await _service.CriarAsync(Input());

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AtualizarAsync(criado.Id, Input(nome: "")));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AtualizarAsync(criado.Id, Input(preco: -1m)));

        Assert.Equal(0, _repository.Atualizacoes);
        Assert.Equal("Óleo 5W30", _repository.Itens.Single().Nome);
    }

    [Fact]
    public async Task Atualizar_DevePersistirUmaVez_QuandoExiste()
    {
        var criado = await _service.CriarAsync(Input());

        await _service.AtualizarAsync(criado.Id, Input(nome: "Óleo 10W40", preco: 35m));

        Assert.Equal(1, _repository.Atualizacoes);
    }

    [Fact]
    public async Task Remover_NaoDeveAfetarOsDemaisItens()
    {
        var oleo = await _service.CriarAsync(Input("Óleo"));
        await _service.CriarAsync(Input("Filtro"));

        await _service.RemoverAsync(oleo.Id);

        Assert.Equal("Filtro", Assert.Single(_repository.Itens).Nome);
    }
}
