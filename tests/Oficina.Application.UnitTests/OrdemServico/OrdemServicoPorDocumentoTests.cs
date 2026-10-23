using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using Oficina.Domain.ValueObjects;
using CatalogoItem = Oficina.Domain.Entities.Catalogo;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Application.UnitTests.OrdemServico;

public class OrdemServicoPorDocumentoTests
{
    private const string CpfMaria = "529.982.247-25";
    private const string PlacaMaria = "ABC1D23";

    private readonly FakeClienteRepository _clientes = new();
    private readonly FakeVeiculoRepository _veiculos = new();
    private readonly FakeOrdemServicoRepository _ordens = new();
    private readonly OrdemServicoService _service;

    public OrdemServicoPorDocumentoTests()
    {
        var maria = DefinirId(new Cliente("Maria", new Documento(CpfMaria)), 10);
        var joao = DefinirId(new Cliente("João", new Documento("11.444.777/0001-61")), 20);
        _clientes.Itens.Add(maria);
        _clientes.Itens.Add(joao);

        _veiculos.Itens.Add(DefinirId(new Veiculo(maria.Id, new Placa(PlacaMaria), "Fiat", "Uno", 2020), 7));
        _veiculos.Itens.Add(DefinirId(new Veiculo(joao.Id, new Placa("DTY0911"), "Ferrari", "Roma", 2025), 8));

        _service = new OrdemServicoService(
            _ordens, _clientes, _veiculos, new NaoUsado(), new NaoUsado(), new NaoUsado());
    }

    private static T DefinirId<T>(T entidade, int id) where T : notnull
    {
        typeof(T).GetProperty("Id")!.SetValue(entidade, id);
        return entidade;
    }

    private static CriarOrdemServicoInputDto Pedido(string documento, string placa)
        => new() { Documento = documento, Placa = placa };

    [Theory]
    [InlineData("529.982.247-25", "ABC1D23")]
    [InlineData("52998224725", "abc-1d23")]
    [InlineData(" 529 982 247 25 ", " abc1d23 ")]
    public async Task Criar_DeveAbrirOrdemRecebida_ParaOClienteEVeiculoIdentificadosPorDocumentoEPlaca(
        string documento, string placa)
    {
        var resultado = await _service.CriarAsync(Pedido(documento, placa));

        Assert.Equal(10, resultado.ClienteId);
        Assert.Equal(7, resultado.VeiculoId);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.Recebida, resultado.Status);
        Assert.Single(_ordens.Itens);
    }

    [Fact]
    public async Task Criar_DeveAceitarCnpj()
    {
        var resultado = await _service.CriarAsync(Pedido("11444777000161", "DTY0911"));

        Assert.Equal(20, resultado.ClienteId);
        Assert.Equal(8, resultado.VeiculoId);
    }

    [Fact]
    public async Task Criar_DeveLancarKeyNotFound_QuandoNaoExisteClienteComODocumento()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CriarAsync(Pedido("111.444.777-35", PlacaMaria)));

        Assert.Empty(_ordens.Itens);
    }

    [Fact]
    public async Task Criar_DeveLancarKeyNotFound_QuandoNaoExisteVeiculoComAPlaca()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CriarAsync(Pedido(CpfMaria, "XYZ9K88")));

        Assert.Empty(_ordens.Itens);
    }

    [Fact]
    public async Task Criar_DeveLancarInvalidOperation_QuandoOVeiculoNaoPertenceAoCliente()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CriarAsync(Pedido(CpfMaria, "DTY0911")));

        Assert.Empty(_ordens.Itens);
    }

    [Theory]
    [InlineData("111.111.111-11", PlacaMaria)]
    [InlineData("123", PlacaMaria)]
    [InlineData(CpfMaria, "XX")]
    [InlineData(CpfMaria, "1234567")]
    public async Task Criar_DeveLancarArgumentException_QuandoDocumentoOuPlacaSaoInvalidos(string documento, string placa)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CriarAsync(Pedido(documento, placa)));

        Assert.Empty(_ordens.Itens);
    }

    #region Consulta por documento
    [Fact]
    public async Task ObterPorDocumento_DeveListarSomenteAsOrdensDoCliente_ComOuSemMascara()
    {
        await _service.CriarAsync(Pedido(CpfMaria, PlacaMaria));
        await _service.CriarAsync(Pedido("52998224725", "abc-1d23"));
        await _service.CriarAsync(Pedido("11.444.777/0001-61", "DTY0911"));

        var daMaria = (await _service.ObterPorDocumentoAsync("529.982.247-25")).ToList();
        var daMariaSemMascara = (await _service.ObterPorDocumentoAsync("52998224725")).ToList();
        var doJoao = (await _service.ObterPorDocumentoAsync("11444777000161")).ToList();

        Assert.Equal(2, daMaria.Count);
        Assert.Equal(2, daMariaSemMascara.Count);
        Assert.All(daMaria, o => Assert.Equal(10, o.ClienteId));
        Assert.Single(doJoao);
        Assert.Equal(20, doJoao[0].ClienteId);
    }

    [Fact]
    public async Task ObterPorDocumento_DeveRetornarListaVazia_QuandoOClienteNaoTemOrdens()
    {
        var resultado = await _service.ObterPorDocumentoAsync(CpfMaria);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterPorDocumento_DeveLancarKeyNotFound_QuandoNaoExisteClienteComODocumento()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ObterPorDocumentoAsync("111.444.777-35"));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("123")]
    [InlineData("abc")]
    public async Task ObterPorDocumento_DeveLancarArgumentException_QuandoDocumentoInvalido(string documento)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ObterPorDocumentoAsync(documento));
    }
    #endregion

    #region Aprovação e rejeição do orçamento por documento
    // OS com o orçamento gerado e disponibilizado ao cliente (aguardando a decisão dele)
    private Ordem CriarOrdemAguardandoDecisao(int clienteId, int veiculoId, int id)
    {
        var ordem = DefinirId(new Ordem(clienteId, veiculoId), id);
        ordem.IniciarDiagnostico();
        ordem.AdicionarServico(1, "Revisão", 100m);
        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        ordem.DisponibilizarOrcamento();
        _ordens.Itens.Add(ordem);
        return ordem;
    }

    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public async Task Aprovar_DeveAprovarOOrcamentoDaOrdemAguardandoDecisao_ComOuSemMascara(string documento)
    {
        var ordem = CriarOrdemAguardandoDecisao(10, 7, 1);

        var resultado = await _service.AprovarOrcamentoAsync(documento);

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, ordem.Orcamento!.Status);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, resultado.Orcamento!.Status);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task Rejeitar_DeveRejeitarOOrcamentoDaOrdemAguardandoDecisao()
    {
        var ordem = CriarOrdemAguardandoDecisao(10, 7, 1);

        await _service.RejeitarOrcamentoAsync(CpfMaria);

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Rejeitado, ordem.Orcamento!.Status);
    }

    [Fact]
    public async Task Aprovar_NaoDeveMexerNaOrdemDeOutroCliente()
    {
        var daMaria = CriarOrdemAguardandoDecisao(10, 7, 1);
        var doJoao = CriarOrdemAguardandoDecisao(20, 8, 2);

        await _service.AprovarOrcamentoAsync(CpfMaria);

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, daMaria.Orcamento!.Status);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, doJoao.Orcamento!.Status);
    }

    [Fact]
    public async Task Aprovar_DeveExigirAPlaca_QuandoOClienteTemMaisDeUmaOrdemAguardando()
    {
        var segundoVeiculo = DefinirId(new Veiculo(10, new Placa("QWE1R23"), "VW", "Gol", 2019), 9);
        _veiculos.Itens.Add(segundoVeiculo);
        var primeira = CriarOrdemAguardandoDecisao(10, 7, 1);
        var segunda = CriarOrdemAguardandoDecisao(10, 9, 2);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AprovarOrcamentoAsync(CpfMaria));
        Assert.Contains("placa", erro.Message);
        Assert.Equal(0, _ordens.Atualizacoes);

        await _service.AprovarOrcamentoAsync(CpfMaria, "qwe-1r23");

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, primeira.Orcamento!.Status);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, segunda.Orcamento!.Status);
    }

    [Fact]
    public async Task Aprovar_DeveLancarInvalidOperation_QuandoNaoHaOrdemAguardandoDecisao()
    {
        // OS ainda "Recebida": o orçamento nem foi disponibilizado
        _ordens.Itens.Add(DefinirId(new Ordem(10, 7), 1));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AprovarOrcamentoAsync(CpfMaria));
    }

    [Fact]
    public async Task Aprovar_DeveLancarInvalidOperation_QuandoODecisaoJaFoiRegistrada()
    {
        CriarOrdemAguardandoDecisao(10, 7, 1);
        await _service.AprovarOrcamentoAsync(CpfMaria);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RejeitarOrcamentoAsync(CpfMaria));
    }

    [Fact]
    public async Task Aprovar_DeveLancarInvalidOperation_QuandoAPlacaEDeVeiculoSemOrdemAguardando()
    {
        CriarOrdemAguardandoDecisao(10, 7, 1);

        // DTY0911 existe, mas é do João e não tem OS aguardando decisão da Maria
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AprovarOrcamentoAsync(CpfMaria, "DTY0911"));
    }

    [Fact]
    public async Task Aprovar_DeveLancarKeyNotFound_QuandoClienteOuVeiculoNaoExistem()
    {
        CriarOrdemAguardandoDecisao(10, 7, 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AprovarOrcamentoAsync("111.444.777-35"));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AprovarOrcamentoAsync(CpfMaria, "XYZ9K88"));
    }

    [Theory]
    [InlineData("111.111.111-11", null)]
    [InlineData("123", null)]
    [InlineData(CpfMaria, "XX")]
    public async Task Aprovar_DeveLancarArgumentException_QuandoDocumentoOuPlacaSaoInvalidos(string documento, string? placa)
    {
        CriarOrdemAguardandoDecisao(10, 7, 1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RejeitarOrcamentoAsync(documento, placa));
    }
    #endregion

    #region Fakes
    private sealed class FakeClienteRepository : IClienteRepository
    {
        public List<Cliente> Itens { get; } = new();

        public Task<Cliente?> ObterPorDocumentoAsync(string numero)
            => Task.FromResult(Itens.FirstOrDefault(c => c.Documento.Numero == numero));

        public Task<IEnumerable<Cliente>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Cliente?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Cliente cliente) => throw new NotImplementedException();
        public Task AtualizarAsync(Cliente cliente) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    private sealed class FakeVeiculoRepository : IVeiculoRepository
    {
        public List<Veiculo> Itens { get; } = new();

        public Task<Veiculo?> ObterPorPlacaAsync(string placa)
            => Task.FromResult(Itens.FirstOrDefault(v => v.Placa.Valor == placa));

        public Task<IEnumerable<Veiculo>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Veiculo?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Veiculo veiculo) => throw new NotImplementedException();
        public Task AtualizarAsync(Veiculo veiculo) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    private sealed class FakeOrdemServicoRepository : IOrdemServicoRepository
    {
        public List<Ordem> Itens { get; } = new();

        public Task AdicionarAsync(Ordem ordemServico)
        {
            Itens.Add(ordemServico);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(int clienteId)
            => Task.FromResult<IEnumerable<Ordem>>(Itens.Where(o => o.ClienteId == clienteId).ToList());

        public Task<IEnumerable<Ordem>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Ordem?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public int Atualizacoes { get; private set; }

        public Task AtualizarAsync(Ordem ordemServico)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    private sealed class NaoUsado : IServicoRepository, ICatalogoRepository, IEstoqueRepository
    {
        Task<IEnumerable<Servico>> IServicoRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<Servico?> IServicoRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task AdicionarAsync(Servico servico) => throw new NotImplementedException();
        public Task AtualizarAsync(Servico servico) => throw new NotImplementedException();

        Task<IEnumerable<CatalogoItem>> ICatalogoRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<CatalogoItem?> ICatalogoRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task AdicionarAsync(CatalogoItem catalogo) => throw new NotImplementedException();
        public Task AtualizarAsync(CatalogoItem catalogo) => throw new NotImplementedException();

        Task<IEnumerable<Estoque>> IEstoqueRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<Estoque?> IEstoqueRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<Estoque?> ObterPorCatalogoIdAsync(int catalogoId) => throw new NotImplementedException();
        public Task AdicionarAsync(Estoque estoque) => throw new NotImplementedException();
        public Task AtualizarAsync(Estoque estoque) => throw new NotImplementedException();

        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }
    #endregion
}
