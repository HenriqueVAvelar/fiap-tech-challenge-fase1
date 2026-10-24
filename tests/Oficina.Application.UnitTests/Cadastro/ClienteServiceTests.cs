using Oficina.Application.DTOs;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Cadastro;

public class ClienteServiceTests
{
    private const string CpfA = "529.982.247-25";
    private const string CpfB = "111.444.777-35";

    private readonly FakeClienteRepository _repository = new();
    private readonly ClienteService _service;

    public ClienteServiceTests()
    {
        _service = new ClienteService(_repository, CadastroFakes.CriarMapper());
    }

    private static ClienteInputDto Input(string nome = "Maria", string documento = CpfA)
        => new() { Nome = nome, Documento = documento };

    [Fact]
    public async Task Criar_DevePersistirComDocumentoSomenteDigitos()
    {
        var dto = await _service.CriarAsync(Input());

        Assert.Equal("52998224725", dto.Documento);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cpf, dto.TipoDocumento);
        Assert.Single(_repository.Clientes);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoDocumentoJaExiste_ComOuSemMascara()
    {
        await _service.CriarAsync(Input());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarAsync(Input("Outra", "52998224725")));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoDocumentoInvalido()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input(documento: "123")));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoNomeVazio()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input(nome: " ")));
    }

    [Fact]
    public async Task ObterTodos_DeveListarOsClientes()
    {
        await _service.CriarAsync(Input("Maria", CpfA));
        await _service.CriarAsync(Input("João", CpfB));

        var clientes = await _service.ObterTodosAsync();

        Assert.Equal(2, clientes.Count());
    }

    [Fact]
    public async Task ObterPorDocumento_DeveEncontrar_ComOuSemMascara()
    {
        await _service.CriarAsync(Input());

        Assert.NotNull(await _service.ObterPorDocumentoAsync("52998224725"));
        Assert.NotNull(await _service.ObterPorDocumentoAsync(CpfA));
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarNull_QuandoNaoExiste()
    {
        Assert.Null(await _service.ObterPorDocumentoAsync(CpfB));
    }

    [Fact]
    public async Task Atualizar_DeveAlterarNomeEDocumento()
    {
        await _service.CriarAsync(Input());

        var ok = await _service.AtualizarAsync(CpfA, Input("Maria Silva", CpfB));

        Assert.True(ok);
        var cliente = _repository.Clientes.Single();
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("11144477735", cliente.Documento.Numero);
    }

    [Fact]
    public async Task Atualizar_DevePermitirManterOProprioDocumento()
    {
        await _service.CriarAsync(Input());

        Assert.True(await _service.AtualizarAsync(CpfA, Input("Novo nome", CpfA)));
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoDocumentoDeOutroCliente()
    {
        await _service.CriarAsync(Input("Maria", CpfA));
        await _service.CriarAsync(Input("João", CpfB));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AtualizarAsync(CpfB, Input("João", CpfA)));
    }

    [Fact]
    public async Task Atualizar_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync(CpfA, Input()));
    }

    [Fact]
    public async Task Remover_DeveExcluirOCliente()
    {
        await _service.CriarAsync(Input());

        Assert.True(await _service.RemoverAsync(CpfA));
        Assert.Empty(_repository.Clientes);
    }

    [Fact]
    public async Task Remover_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.RemoverAsync(CpfA));
    }
}
