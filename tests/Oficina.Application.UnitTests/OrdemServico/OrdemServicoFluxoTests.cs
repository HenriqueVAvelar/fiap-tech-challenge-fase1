using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using CatalogoItem = Oficina.Domain.Entities.Catalogo;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Application.UnitTests.OrdemServico;

public class OrdemServicoFluxoTests
{
    private const int OrdemId = 1;
    private const int ServicoId = 3;
    private const int Filtro = 7;

    private readonly FakeOrdemServicoRepository _ordens = new();
    private readonly FakeServicoRepository _servicos = new();
    private readonly FakeCatalogoRepository _catalogo = new();
    private readonly FakeEstoqueRepository _estoques = new();
    private readonly OrdemServicoService _service;

    public OrdemServicoFluxoTests()
    {
        _servicos.Servicos.Add(DefinirId(
            new Servico { Descricao = "Troca de óleo", Valor = 100m, Ativo = true }, ServicoId));
        _servicos.Servicos.Add(DefinirId(
            new Servico { Descricao = "Serviço antigo", Valor = 10m, Ativo = false }, 4));

        _catalogo.Itens.Add(DefinirId(
            new CatalogoItem("Filtro de óleo", 50m, EnumsConsumivel.TipoConsumivel.Peca, true), Filtro));
        _catalogo.Itens.Add(DefinirId(
            new CatalogoItem("Peça descontinuada", 20m, EnumsConsumivel.TipoConsumivel.Peca, false), 8));

        _service = new OrdemServicoService(
            _ordens,
            new FakeClienteRepository(),
            new FakeVeiculoRepository(),
            _servicos,
            _catalogo,
            _estoques);
    }

    private static T DefinirId<T>(T entidade, int id) where T : notnull
    {
        typeof(T).GetProperty("Id")!.SetValue(entidade, id);
        return entidade;
    }

    private Ordem CriarOrdemRecebida()
    {
        var ordem = DefinirId(new Ordem(10, 20), OrdemId);
        _ordens.Itens.Add(ordem);
        return ordem;
    }

    private Ordem CriarOrdemEmDiagnostico()
    {
        var ordem = CriarOrdemRecebida();
        ordem.IniciarDiagnostico();
        return ordem;
    }

    private Ordem CriarOrdemComOrcamento()
    {
        var ordem = CriarOrdemEmDiagnostico();
        ordem.AdicionarServico(ServicoId, "Troca de óleo", 100m);
        ordem.AdicionarPecaInsumo(Filtro, "Filtro de óleo", EnumsConsumivel.TipoConsumivel.Peca, 2, 25m);
        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        return ordem;
    }

    private Ordem CriarOrdemAguardandoAprovacao()
    {
        var ordem = CriarOrdemComOrcamento();
        ordem.DisponibilizarOrcamento();
        return ordem;
    }

    private Ordem CriarOrdemEmExecucao()
    {
        var ordem = CriarOrdemAguardandoAprovacao();
        ordem.AprovarOrcamento();
        ordem.IniciarExecucao();
        return ordem;
    }

    #region Consulta
    [Fact]
    public async Task ObterTodos_DeveMapearTodasAsOrdens()
    {
        CriarOrdemRecebida();
        _ordens.Itens.Add(DefinirId(new Ordem(11, 21), 2));

        var resultado = (await _service.ObterTodosAsync()).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal(new[] { 1, 2 }, resultado.Select(o => o.Id));
        Assert.Equal(new[] { 10, 11 }, resultado.Select(o => o.ClienteId));
        Assert.Equal(new[] { 20, 21 }, resultado.Select(o => o.VeiculoId));
        Assert.All(resultado, o => Assert.Equal(EnumsOrdemServico.StatusOrdemServico.Recebida, o.Status));
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarListaVazia_QuandoNaoHaOrdens()
    {
        var resultado = await _service.ObterTodosAsync();

        Assert.Empty(resultado);
    }
    #endregion

    #region Adicionar serviço
    [Fact]
    public async Task AdicionarServico_DeveIncluirOServicoComDescricaoEValorDoCadastro()
    {
        CriarOrdemRecebida();

        var resultado = await _service.AdicionarServicoAsync(
            OrdemId, new AdicionarServicoOrdemInputDto { ServicoId = ServicoId });

        var item = Assert.Single(resultado.ItensServico);
        Assert.Equal(ServicoId, item.ServicoId);
        Assert.Equal("Troca de óleo", item.Descricao);
        Assert.Equal(100m, item.Valor);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarServico_DeveLancarKeyNotFound_QuandoAOrdemNaoExiste()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AdicionarServicoAsync(99, new AdicionarServicoOrdemInputDto { ServicoId = ServicoId }));
    }

    [Fact]
    public async Task AdicionarServico_DeveLancarKeyNotFound_QuandoOServicoNaoExiste()
    {
        var ordem = CriarOrdemRecebida();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AdicionarServicoAsync(OrdemId, new AdicionarServicoOrdemInputDto { ServicoId = 99 }));

        Assert.Empty(ordem.ItensServico);
        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarServico_DeveLancarInvalidOperation_QuandoOServicoEstaInativo()
    {
        var ordem = CriarOrdemRecebida();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarServicoAsync(OrdemId, new AdicionarServicoOrdemInputDto { ServicoId = 4 }));

        Assert.Empty(ordem.ItensServico);
        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarServico_DeveLancarInvalidOperation_AposAGeracaoDoOrcamento()
    {
        var ordem = CriarOrdemComOrcamento();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarServicoAsync(OrdemId, new AdicionarServicoOrdemInputDto { ServicoId = ServicoId }));

        Assert.Single(ordem.ItensServico);
        Assert.Equal(0, _ordens.Atualizacoes);
    }
    #endregion

    #region Adicionar peça ou insumo
    [Fact]
    public async Task AdicionarPeca_DeveIncluirOItemComDadosDoCatalogo_SemAlterarOEstoque()
    {
        var estoque = new Estoque(10) { CatalogoId = Filtro };
        _estoques.Estoques.Add(estoque);
        CriarOrdemEmDiagnostico();

        var resultado = await _service.AdicionarPecaInsumoAsync(
            OrdemId, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = Filtro, Quantidade = 3 });

        var item = Assert.Single(resultado.ItensPecaInsumo);
        Assert.Equal(Filtro, item.CatalogoId);
        Assert.Equal("Filtro de óleo", item.Nome);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, item.Tipo);
        Assert.Equal(3, item.Quantidade);
        Assert.Equal(50m, item.ValorUnitario);
        Assert.Equal(150m, item.ValorTotal);
        Assert.False(item.Consumido);
        Assert.Equal(10, estoque.Quantidade);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarPeca_DeveLancarKeyNotFound_QuandoAOrdemNaoExiste()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AdicionarPecaInsumoAsync(
                99, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = Filtro, Quantidade = 1 }));
    }

    [Fact]
    public async Task AdicionarPeca_DeveLancarKeyNotFound_QuandoOItemDeCatalogoNaoExiste()
    {
        CriarOrdemEmDiagnostico();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AdicionarPecaInsumoAsync(
                OrdemId, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = 99, Quantidade = 1 }));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarPeca_DeveLancarInvalidOperation_QuandoOItemDeCatalogoEstaInativo()
    {
        _estoques.Estoques.Add(new Estoque(10) { CatalogoId = 8 });
        var ordem = CriarOrdemEmDiagnostico();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarPecaInsumoAsync(
                OrdemId, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = 8, Quantidade = 1 }));

        Assert.Empty(ordem.ItensPecaInsumo);
        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task AdicionarPeca_DeveLancarInvalidOperation_QuandoOrdemNaoEstaEmDiagnostico()
    {
        _estoques.Estoques.Add(new Estoque(10) { CatalogoId = Filtro });
        CriarOrdemRecebida();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarPecaInsumoAsync(
                OrdemId, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = Filtro, Quantidade = 1 }));

        Assert.Equal(0, _ordens.Atualizacoes);
    }
    #endregion

    #region Transições de status
    [Fact]
    public async Task IniciarDiagnostico_DeveAlterarOStatus_EPersistir()
    {
        CriarOrdemRecebida();

        var resultado = await _service.IniciarDiagnosticoAsync(OrdemId);

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.EmDiagnostico, resultado.Status);
        Assert.NotNull(resultado.DataInicioDiagnostico);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task ConcluirDiagnostico_DeveRegistrarADataDeConclusao_EPersistir()
    {
        CriarOrdemEmDiagnostico();

        var resultado = await _service.ConcluirDiagnosticoAsync(OrdemId);

        Assert.NotNull(resultado.DataConclusaoDiagnostico);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.EmDiagnostico, resultado.Status);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task GerarOrcamento_DeveCalcularOsValores_EPersistir()
    {
        var ordem = CriarOrdemEmDiagnostico();
        ordem.AdicionarServico(ServicoId, "Troca de óleo", 100m);
        ordem.AdicionarPecaInsumo(Filtro, "Filtro de óleo", EnumsConsumivel.TipoConsumivel.Peca, 2, 25m);
        ordem.ConcluirDiagnostico();

        var resultado = await _service.GerarOrcamentoAsync(OrdemId);

        var orcamento = resultado.Orcamento!;
        Assert.Equal(100m, orcamento.ValorServicos);
        Assert.Equal(50m, orcamento.ValorPecasInsumos);
        Assert.Equal(150m, orcamento.ValorTotal);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, orcamento.Status);
        Assert.Null(orcamento.DataDisponibilizacao);
        Assert.Null(orcamento.DataDecisao);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task GerarOrcamento_DeveLancarInvalidOperation_QuandoODiagnosticoNaoFoiConcluido()
    {
        var ordem = CriarOrdemEmDiagnostico();
        ordem.AdicionarServico(ServicoId, "Troca de óleo", 100m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GerarOrcamentoAsync(OrdemId));

        Assert.Null(ordem.Orcamento);
        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task DisponibilizarOrcamento_DeveAlterarOStatusParaAguardandoAprovacao()
    {
        CriarOrdemComOrcamento();

        var resultado = await _service.DisponibilizarOrcamentoAsync(OrdemId);

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao, resultado.Status);
        Assert.NotNull(resultado.Orcamento!.DataDisponibilizacao);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task DisponibilizarOrcamento_DeveLancarInvalidOperation_QuandoOrcamentoNaoFoiGerado()
    {
        CriarOrdemEmDiagnostico();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DisponibilizarOrcamentoAsync(OrdemId));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task IniciarExecucao_DeveAlterarOStatusParaEmExecucao_AposAAprovacao()
    {
        var ordem = CriarOrdemAguardandoAprovacao();
        ordem.AprovarOrcamento();

        var resultado = await _service.IniciarExecucaoAsync(OrdemId);

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.EmExecucao, resultado.Status);
        Assert.NotNull(resultado.DataInicioExecucao);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task IniciarExecucao_DeveLancarInvalidOperation_SemAAprovacaoDoOrcamento()
    {
        CriarOrdemAguardandoAprovacao();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.IniciarExecucaoAsync(OrdemId));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task IniciarExecucao_DeveLancarInvalidOperation_QuandoOOrcamentoFoiRejeitado()
    {
        var ordem = CriarOrdemAguardandoAprovacao();
        ordem.RejeitarOrcamento();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.IniciarExecucaoAsync(OrdemId));
    }

    [Fact]
    public async Task Finalizar_DeveAlterarOStatusParaFinalizada_EPersistir()
    {
        CriarOrdemEmExecucao();

        var resultado = await _service.FinalizarAsync(OrdemId);

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.Finalizada, resultado.Status);
        Assert.NotNull(resultado.DataFinalizacao);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task Finalizar_DeveLancarInvalidOperation_QuandoOrdemNaoEstaEmExecucao()
    {
        CriarOrdemRecebida();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.FinalizarAsync(OrdemId));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task Entregar_DeveAlterarOStatusParaEntregue_EPersistir()
    {
        var ordem = CriarOrdemEmExecucao();
        ordem.Finalizar();

        var resultado = await _service.EntregarAsync(OrdemId);

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.Entregue, resultado.Status);
        Assert.NotNull(resultado.DataEntrega);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task Entregar_DeveLancarInvalidOperation_QuandoOrdemNaoEstaFinalizada()
    {
        CriarOrdemEmExecucao();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EntregarAsync(OrdemId));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task Transicoes_DevemLancarKeyNotFound_QuandoAOrdemNaoExiste()
    {
        const int inexistente = 99;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.IniciarDiagnosticoAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConcluirDiagnosticoAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GerarOrcamentoAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DisponibilizarOrcamentoAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.IniciarExecucaoAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.FinalizarAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EntregarAsync(inexistente));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RegistrarConsumoAsync(inexistente, new RegistrarConsumoOrdemInputDto { ItensIds = new List<int> { 1 } }));

        Assert.Equal(0, _ordens.Atualizacoes);
    }
    #endregion

    #region Mapeamento
    [Fact]
    public async Task Mapeamento_DeveIncluirItensEOrcamentoNoDto()
    {
        var ordem = CriarOrdemAguardandoAprovacao();
        ordem.AprovarOrcamento();

        var resultado = (await _service.ObterTodosAsync()).Single();

        Assert.Equal(OrdemId, resultado.Id);
        Assert.Equal(10, resultado.ClienteId);
        Assert.Equal(20, resultado.VeiculoId);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao, resultado.Status);
        Assert.Equal(ordem.DataCriacao, resultado.DataCriacao);
        Assert.Equal(ordem.DataInicioDiagnostico, resultado.DataInicioDiagnostico);
        Assert.Equal(ordem.DataConclusaoDiagnostico, resultado.DataConclusaoDiagnostico);
        Assert.Null(resultado.DataInicioExecucao);
        Assert.Null(resultado.DataFinalizacao);
        Assert.Null(resultado.DataEntrega);

        var servico = Assert.Single(resultado.ItensServico);
        Assert.Equal("Troca de óleo", servico.Descricao);

        var peca = Assert.Single(resultado.ItensPecaInsumo);
        Assert.Equal(50m, peca.ValorTotal);

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, resultado.Orcamento!.Status);
        Assert.Equal(150m, resultado.Orcamento.ValorTotal);
        Assert.NotNull(resultado.Orcamento.DataGeracao);
        Assert.NotNull(resultado.Orcamento.DataDisponibilizacao);
        Assert.NotNull(resultado.Orcamento.DataDecisao);
    }

    [Fact]
    public async Task Mapeamento_DeveDeixarOOrcamentoNulo_QuandoAindaNaoFoiGerado()
    {
        CriarOrdemRecebida();

        var resultado = (await _service.ObterTodosAsync()).Single();

        Assert.Null(resultado.Orcamento);
        Assert.Empty(resultado.ItensServico);
        Assert.Empty(resultado.ItensPecaInsumo);
    }
    #endregion

    private sealed class FakeOrdemServicoRepository : IOrdemServicoRepository
    {
        public List<Ordem> Itens { get; } = new();
        public int Atualizacoes { get; private set; }

        public Task<IEnumerable<Ordem>> ObterTodosAsync()
            => Task.FromResult<IEnumerable<Ordem>>(Itens.ToList());

        public Task<Ordem?> ObterPorIdAsync(int id)
            => Task.FromResult(Itens.FirstOrDefault(o => o.Id == id));

        public Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(int clienteId)
            => Task.FromResult<IEnumerable<Ordem>>(Itens.Where(o => o.ClienteId == clienteId).ToList());

        public Task AdicionarAsync(Ordem ordemServico)
        {
            Itens.Add(ordemServico);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Ordem ordemServico)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(o => o.Id == id);
            return Task.CompletedTask;
        }
    }
}
