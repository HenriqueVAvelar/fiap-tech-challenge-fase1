using Oficina.Application.DTOs;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.UnitTests.Cadastro;

public class VeiculoServiceTests
{
    private readonly FakeVeiculoRepository _repository = new();
    private readonly FakeClienteRepository _clientes = new();
    private readonly VeiculoService _service;
    private readonly int _clienteId;

    public VeiculoServiceTests()
    {
        _service = new VeiculoService(_repository, _clientes, CadastroFakes.CriarMapper());

        var cliente = new Cliente("Maria", new Documento("529.982.247-25"));
        _clientes.AdicionarAsync(cliente).GetAwaiter().GetResult();
        _clienteId = cliente.Id;
    }

    private VeiculoInputDto Input(string placa = "ABC-1234", int? clienteId = null)
        => new()
        {
            ClienteId = clienteId ?? _clienteId,
            Placa = placa,
            Marca = " Fiat ",
            Modelo = " Uno ",
            Ano = 2020
        };

    [Fact]
    public async Task Criar_DevePersistirComPlacaNormalizada_ETextosSemEspacos()
    {
        var dto = await _service.CriarAsync(Input("abc-1234"));

        Assert.Equal("ABC1234", dto.Placa);
        Assert.Equal("Fiat", dto.Marca);
        Assert.Equal("Uno", dto.Modelo);
        Assert.Single(_repository.Veiculos);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CriarAsync(Input(clienteId: 99)));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoPlacaJaExiste()
    {
        await _service.CriarAsync(Input("ABC1234"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarAsync(Input("abc-1234")));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoPlacaInvalida()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input("XX")));
    }

    [Fact]
    public async Task ObterTodos_DeveListarOsVeiculos()
    {
        await _service.CriarAsync(Input("ABC1234"));
        await _service.CriarAsync(Input("DEF5678"));

        Assert.Equal(2, (await _service.ObterTodosAsync()).Count());
    }

    [Fact]
    public async Task ObterPorPlaca_DeveEncontrar_ComOuSemMascara()
    {
        await _service.CriarAsync(Input("ABC1234"));

        Assert.NotNull(await _service.ObterPorPlacaAsync("abc-1234"));
        Assert.Null(await _service.ObterPorPlacaAsync("ZZZ9999"));
    }

    [Fact]
    public async Task Atualizar_DeveAlterarOsDados()
    {
        await _service.CriarAsync(Input("ABC1234"));

        var ok = await _service.AtualizarAsync("ABC1234", Input("DEF5678"));

        Assert.True(ok);
        Assert.Equal("DEF5678", _repository.Veiculos.Single().Placa.Valor);
    }

    [Fact]
    public async Task Atualizar_DevePermitirManterAPropriaPlaca()
    {
        await _service.CriarAsync(Input("ABC1234"));

        Assert.True(await _service.AtualizarAsync("ABC1234", Input("ABC-1234")));
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoPlacaDeOutroVeiculo()
    {
        await _service.CriarAsync(Input("ABC1234"));
        await _service.CriarAsync(Input("DEF5678"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AtualizarAsync("DEF5678", Input("ABC1234")));
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoNovoClienteNaoExiste()
    {
        await _service.CriarAsync(Input("ABC1234"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AtualizarAsync("ABC1234", Input("ABC1234", clienteId: 99)));
    }

    [Fact]
    public async Task Atualizar_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync("ABC1234", Input()));
    }

    [Fact]
    public async Task Remover_DeveExcluirOVeiculo()
    {
        await _service.CriarAsync(Input("ABC1234"));

        Assert.True(await _service.RemoverAsync("abc-1234"));
        Assert.Empty(_repository.Veiculos);
    }

    [Fact]
    public async Task Remover_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.RemoverAsync("ABC1234"));
    }
}
