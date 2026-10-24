using Oficina.Application.DTOs;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;

namespace Oficina.Application.UnitTests.Cadastro;

public class ServicoServiceTests
{
    private readonly FakeServicoRepository _repository = new();
    private readonly ServicoService _service;

    public ServicoServiceTests()
    {
        _service = new ServicoService(_repository, CadastroFakes.CriarMapper());
    }

    private static ServicoInputDto Input(string descricao = "Revisão", decimal valor = 100m, bool ativo = true)
        => new() { Descricao = descricao, Valor = valor, Ativo = ativo };

    [Fact]
    public async Task Criar_DevePersistirComDescricaoSemEspacos()
    {
        var dto = await _service.CriarAsync(Input("  Revisão  "));

        Assert.Equal("Revisão", dto.Descricao);
        Assert.Equal(100m, dto.Valor);
        Assert.True(dto.Ativo);
        Assert.Single(_repository.Servicos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_DeveLancarExcecao_QuandoDescricaoVazia(string descricao)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input(descricao)));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoValorNegativo()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input(valor: -1m)));
    }

    [Fact]
    public async Task ObterTodos_EObterPorId_DevemRetornarOsServicos()
    {
        var criado = await _service.CriarAsync(Input());

        Assert.Single(await _service.ObterTodosAsync());
        Assert.Equal("Revisão", (await _service.ObterPorIdAsync(criado.Id))!.Descricao);
        Assert.Null(await _service.ObterPorIdAsync(42));
    }

    [Fact]
    public async Task Atualizar_DeveAlterarOServico()
    {
        var criado = await _service.CriarAsync(Input());

        var ok = await _service.AtualizarAsync(criado.Id, Input(" Alinhamento ", 80m, false));

        Assert.True(ok);
        var servico = _repository.Servicos.Single();
        Assert.Equal("Alinhamento", servico.Descricao);
        Assert.Equal(80m, servico.Valor);
        Assert.False(servico.Ativo);
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoDadosInvalidos()
    {
        var criado = await _service.CriarAsync(Input());

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AtualizarAsync(criado.Id, Input(valor: -5m)));
        Assert.Equal(100m, _repository.Servicos.Single().Valor);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync(42, Input()));
    }

    [Fact]
    public async Task Remover_DeveExcluir_QuandoExiste_EFalseQuandoNao()
    {
        var criado = await _service.CriarAsync(Input());

        Assert.True(await _service.RemoverAsync(criado.Id));
        Assert.Empty(_repository.Servicos);
        Assert.False(await _service.RemoverAsync(criado.Id));
    }
}
