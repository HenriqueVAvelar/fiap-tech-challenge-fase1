using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Cadastro;

public class CatalogoServiceTests
{
    private readonly FakeCatalogoRepository _repository = new();
    private readonly CatalogoService _service;

    public CatalogoServiceTests()
    {
        _service = new CatalogoService(_repository, CadastroFakes.CriarMapper());
    }

    private static CatalogoInputDto Input(string nome = "Alternador", decimal preco = 900m)
        => new()
        {
            Nome = nome,
            Preco = preco,
            Tipo = EnumsConsumivel.TipoConsumivel.Peca,
            Ativo = true,
            Descricao = "Descrição"
        };

    [Fact]
    public async Task Criar_DevePersistirEDevolverODto()
    {
        var dto = await _service.CriarAsync(Input());

        Assert.Equal(1, dto.Id);
        Assert.Equal("Alternador", dto.Nome);
        Assert.Equal(900m, dto.Preco);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, dto.Tipo);
        Assert.Single(_repository.Itens);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoPrecoNegativo()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => _service.CriarAsync(Input(preco: -1m)));

        Assert.Empty(_repository.Itens);
    }

    [Fact]
    public async Task ObterTodos_DeveListarTodosOsItens()
    {
        await _service.CriarAsync(Input("A"));
        await _service.CriarAsync(Input("B"));

        var itens = await _service.ObterTodosAsync();

        Assert.Equal(new[] { "A", "B" }, itens.Select(i => i.Nome));
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarOItem_QuandoExiste()
    {
        var criado = await _service.CriarAsync(Input());

        var dto = await _service.ObterPorIdAsync(criado.Id);

        Assert.NotNull(dto);
        Assert.Equal("Alternador", dto!.Nome);
    }

    [Fact]
    public async Task Atualizar_DeveAlterarOItem_QuandoExiste()
    {
        var criado = await _service.CriarAsync(Input());

        var ok = await _service.AtualizarAsync(criado.Id, Input("Bateria", 500m));

        Assert.True(ok);
        Assert.Equal("Bateria", _repository.Itens.Single().Nome);
        Assert.Equal(500m, _repository.Itens.Single().Preco);
        Assert.Equal(1, _repository.Atualizacoes);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync(42, Input()));
        Assert.Equal(0, _repository.Atualizacoes);
    }

    [Fact]
    public async Task Remover_DeveExcluirOItem()
    {
        var criado = await _service.CriarAsync(Input());

        await _service.RemoverAsync(criado.Id);

        Assert.Empty(_repository.Itens);
    }
}
