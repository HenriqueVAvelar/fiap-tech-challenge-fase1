using Oficina.Application.DTOs;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Entities;
using CatalogoEntity = Oficina.Domain.Entities.Catalogo;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Cadastro;

public class EstoqueServiceTests
{
    private readonly FakeEstoqueRepository _repository = new();
    private readonly FakeCatalogoRepository _catalogos = new();
    private readonly EstoqueService _service;
    private readonly int _catalogoId;

    public EstoqueServiceTests()
    {
        _service = new EstoqueService(_repository, _catalogos, CadastroFakes.CriarMapper());

        var catalogo = new CatalogoEntity("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca, true);
        _catalogos.AdicionarAsync(catalogo).GetAwaiter().GetResult();
        _catalogoId = catalogo.Id;
    }

    private Task<EstoqueDto> CriarEstoque(int quantidade = 10)
        => _service.CriarAsync(new EstoqueInputDto { CatalogoId = _catalogoId, Quantidade = quantidade });

    [Fact]
    public async Task Criar_DevePersistirComAQuantidadeInicial()
    {
        var dto = await CriarEstoque(10);

        Assert.Equal(_catalogoId, dto.CatalogoId);
        Assert.Equal(10, dto.Quantidade);
        Assert.Single(_repository.Estoques);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoCatalogoNaoExiste()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CriarAsync(new EstoqueInputDto { CatalogoId = 99, Quantidade = 1 }));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoQuantidadeNegativa()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => CriarEstoque(-1));
    }

    [Fact]
    public async Task ObterTodos_EObterPorId_DevemRetornarOsEstoques()
    {
        var criado = await CriarEstoque();

        Assert.Single(await _service.ObterTodosAsync());
        Assert.Equal(10, (await _service.ObterPorIdAsync(criado.Id))!.Quantidade);
        Assert.Null(await _service.ObterPorIdAsync(42));
    }

    [Fact]
    public async Task Atualizar_DeveAjustarAQuantidade()
    {
        var criado = await CriarEstoque();

        Assert.True(await _service.AtualizarAsync(criado.Id, new EstoqueUpdateInputDto { Quantidade = 3 }));
        Assert.Equal(3, _repository.Estoques.Single().Quantidade);
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoQuantidadeNegativa()
    {
        var criado = await CriarEstoque();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.AtualizarAsync(criado.Id, new EstoqueUpdateInputDto { Quantidade = -1 }));
        Assert.Equal(10, _repository.Estoques.Single().Quantidade);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync(42, new EstoqueUpdateInputDto { Quantidade = 1 }));
    }

    [Fact]
    public async Task Remover_DeveExcluir_QuandoExiste_EFalseQuandoNao()
    {
        var criado = await CriarEstoque();

        Assert.True(await _service.RemoverAsync(criado.Id));
        Assert.Empty(_repository.Estoques);
        Assert.False(await _service.RemoverAsync(criado.Id));
    }

    [Fact]
    public async Task RegistrarEntrada_DeveSomarAoSaldo()
    {
        var criado = await CriarEstoque(10);

        var dto = await _service.RegistrarEntradaAsync(criado.Id, 5);

        Assert.Equal(15, dto!.Quantidade);
        Assert.Equal(15, _repository.Estoques.Single().Quantidade);
    }

    [Fact]
    public async Task RegistrarEntrada_DeveLancarExcecao_QuandoQuantidadeNaoPositiva()
    {
        var criado = await CriarEstoque();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.RegistrarEntradaAsync(criado.Id, 0));
    }

    [Fact]
    public async Task RegistrarBaixa_DeveSubtrairDoSaldo()
    {
        var criado = await CriarEstoque(10);

        var dto = await _service.RegistrarBaixaAsync(criado.Id, 4);

        Assert.Equal(6, dto!.Quantidade);
    }

    [Fact]
    public async Task RegistrarBaixa_DeveLancarExcecao_QuandoSaldoInsuficiente()
    {
        var criado = await CriarEstoque(10);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegistrarBaixaAsync(criado.Id, 11));
        Assert.Equal(10, _repository.Estoques.Single().Quantidade);
    }

    [Fact]
    public async Task EntradaEBaixa_DevemRetornarNull_QuandoNaoEncontrado()
    {
        Assert.Null(await _service.RegistrarEntradaAsync(42, 1));
        Assert.Null(await _service.RegistrarBaixaAsync(42, 1));
    }
}
