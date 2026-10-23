using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using CatalogoItem = Oficina.Domain.Entities.Catalogo;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Application.UnitTests.OrdemServico;

public class OrdemServicoEstoqueTests
{
    private const int OrdemId = 1;
    private const int Filtro = 7;
    private const int Oleo = 8;

    private readonly FakeOrdemServicoRepository _ordens = new();
    private readonly FakeCatalogoRepository _catalogo = new();
    private readonly FakeEstoqueRepository _estoques = new();
    private readonly OrdemServicoService _service;

    public OrdemServicoEstoqueTests()
    {
        _catalogo.Itens.Add(DefinirId(
            new CatalogoItem("Filtro de óleo", 50m, EnumsConsumivel.TipoConsumivel.Peca, true), Filtro));
        _catalogo.Itens.Add(DefinirId(
            new CatalogoItem("Óleo 5W30", 30m, EnumsConsumivel.TipoConsumivel.Insumo, true), Oleo));

        _service = new OrdemServicoService(
            _ordens, new NaoUsado(), new NaoUsado(), new NaoUsado(), _catalogo, _estoques);
    }

    private static T DefinirId<T>(T entidade, int id) where T : notnull
    {
        typeof(T).GetProperty("Id")!.SetValue(entidade, id);
        return entidade;
    }

    private Estoque CriarEstoque(int catalogoId, int saldo)
    {
        var estoque = new Estoque(saldo) { CatalogoId = catalogoId };
        _estoques.Itens.Add(estoque);
        return estoque;
    }

    private Ordem CriarOrdemEmDiagnostico()
    {
        var ordem = DefinirId(new Ordem(1, 1), OrdemId);
        ordem.IniciarDiagnostico();
        _ordens.Itens.Add(ordem);
        return ordem;
    }

    // OS "Em execução" com um item para cada (produto, quantidade); os itens recebem os ids 1, 2, 3...
    private Ordem CriarOrdemEmExecucao(params (int Catalogo, int Quantidade)[] itens)
    {
        var ordem = CriarOrdemEmDiagnostico();
        ordem.AdicionarServico(1, "Troca de óleo", 100m);

        foreach (var (catalogo, quantidade) in itens)
            ordem.AdicionarPecaInsumo(catalogo, $"Produto {catalogo}", EnumsConsumivel.TipoConsumivel.Peca, quantidade, 50m);

        var id = 1;
        foreach (var item in ordem.ItensPecaInsumo)
            DefinirId(item, id++);

        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        ordem.DisponibilizarOrcamento();
        ordem.AprovarOrcamento();
        ordem.IniciarExecucao();
        return ordem;
    }

    private static AdicionarPecaInsumoOrdemInputDto Pedido(int catalogoId, int quantidade)
        => new() { CatalogoId = catalogoId, Quantidade = quantidade };

    private static RegistrarConsumoOrdemInputDto Consumo(params int[] itensIds)
        => new() { ItensIds = itensIds.ToList() };

    #region Inclusão na OS (só confere o saldo)
    [Fact]
    public async Task AdicionarPeca_DeveLancarExcecao_QuandoNaoExisteEstoqueCadastrado()
    {
        CriarOrdemEmDiagnostico();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Filtro, 1)));
    }

    [Fact]
    public async Task AdicionarPeca_DeveLancarExcecao_QuandoSaldoInsuficiente()
    {
        CriarEstoque(Filtro, 3);
        var ordem = CriarOrdemEmDiagnostico();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Filtro, 4)));

        Assert.Empty(ordem.ItensPecaInsumo);
    }

    [Fact]
    public async Task AdicionarPeca_DeveConsiderarOQueJaFoiIncluidoDoMesmoProdutoNaMesmaOrdem()
    {
        CriarEstoque(Filtro, 5);
        CriarOrdemEmDiagnostico();

        await _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Filtro, 3));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Filtro, 3)));
    }

    [Fact]
    public async Task AdicionarPeca_DevePermitirProdutosDiferentes_ENaoAlteraOEstoque()
    {
        var filtro = CriarEstoque(Filtro, 5);
        var oleo = CriarEstoque(Oleo, 4);
        CriarOrdemEmDiagnostico();

        await _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Filtro, 5));
        var resultado = await _service.AdicionarPecaInsumoAsync(OrdemId, Pedido(Oleo, 4));

        Assert.Equal(2, resultado.ItensPecaInsumo.Count());
        Assert.Equal(5, filtro.Quantidade);
        Assert.Equal(4, oleo.Quantidade);
    }
    #endregion

    #region Consumo em lista (baixa no estoque)
    [Fact]
    public async Task RegistrarConsumo_DeveBaixarCadaProduto_EMarcarOsItensDaLista()
    {
        var filtro = CriarEstoque(Filtro, 10);
        var oleo = CriarEstoque(Oleo, 6);
        CriarOrdemEmExecucao((Filtro, 4), (Oleo, 2));

        var resultado = await _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 2));

        Assert.Equal(6, filtro.Quantidade);
        Assert.Equal(4, oleo.Quantidade);
        Assert.All(resultado.ItensPecaInsumo, i => Assert.True(i.Consumido));
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveConsumirSomenteOsItensInformados()
    {
        var filtro = CriarEstoque(Filtro, 10);
        var oleo = CriarEstoque(Oleo, 6);
        CriarOrdemEmExecucao((Filtro, 4), (Oleo, 2));

        var resultado = await _service.RegistrarConsumoAsync(OrdemId, Consumo(2));

        Assert.Equal(10, filtro.Quantidade);
        Assert.Equal(4, oleo.Quantidade);
        Assert.Equal(new[] { false, true }, resultado.ItensPecaInsumo.Select(i => i.Consumido));
    }

    [Fact]
    public async Task RegistrarConsumo_NaoDeveGravarNada_QuandoUmDosItensNaoTemSaldo()
    {
        CriarEstoque(Filtro, 10);
        CriarEstoque(Oleo, 1);
        CriarOrdemEmExecucao((Filtro, 4), (Oleo, 2));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 2)));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveSomarItensDoMesmoProduto_ERecusarQuandoOSaldoNaoCobre()
    {
        CriarEstoque(Filtro, 4); // 2 + 3 = 5 > 4
        CriarOrdemEmExecucao((Filtro, 2), (Filtro, 3));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 2)));

        Assert.Equal(0, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveSomarItensDoMesmoProduto_EZerarOSaldoQuandoCobre()
    {
        var filtro = CriarEstoque(Filtro, 5); // 2 + 3 = 5
        CriarOrdemEmExecucao((Filtro, 2), (Filtro, 3));

        await _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 2));

        Assert.Equal(0, filtro.Quantidade);
        Assert.Equal(1, _ordens.Atualizacoes);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveBaixarUmaVezSo_QuandoOIdVemRepetido()
    {
        var filtro = CriarEstoque(Filtro, 10);
        CriarOrdemEmExecucao((Filtro, 4));

        await _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 1));

        Assert.Equal(6, filtro.Quantidade);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveLancarExcecao_QuandoAListaEstaVazia()
    {
        CriarEstoque(Filtro, 10);
        CriarOrdemEmExecucao((Filtro, 4));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo()));
    }

    [Fact]
    public async Task RegistrarConsumo_DeveLancarExcecao_QuandoNaoExisteEstoqueCadastrado()
    {
        CriarOrdemEmExecucao((Filtro, 1));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(1)));
    }

    [Fact]
    public async Task RegistrarConsumo_DeveLancarExcecao_QuandoOItemJaFoiConsumido()
    {
        var filtro = CriarEstoque(Filtro, 10);
        CriarOrdemEmExecucao((Filtro, 4));
        await _service.RegistrarConsumoAsync(OrdemId, Consumo(1));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(1)));

        Assert.Equal(6, filtro.Quantidade);
    }

    [Fact]
    public async Task RegistrarConsumo_DeveLancarExcecao_QuandoOItemNaoExisteNaOrdem()
    {
        CriarEstoque(Filtro, 10);
        CriarOrdemEmExecucao((Filtro, 4));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(1, 99)));
    }

    [Fact]
    public async Task RegistrarConsumo_DeveLancarExcecao_QuandoAOrdemNaoEstaEmExecucao()
    {
        CriarEstoque(Filtro, 10);
        var ordem = CriarOrdemEmDiagnostico();
        ordem.AdicionarPecaInsumo(Filtro, "Filtro de óleo", EnumsConsumivel.TipoConsumivel.Peca, 1, 50m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarConsumoAsync(OrdemId, Consumo(0)));
    }
    #endregion

    #region Fakes
    private sealed class FakeOrdemServicoRepository : IOrdemServicoRepository
    {
        public List<Ordem> Itens { get; } = new();
        public int Atualizacoes { get; private set; }

        public Task<IEnumerable<Ordem>> ObterTodosAsync()
            => Task.FromResult<IEnumerable<Ordem>>(Itens.ToList());

        public Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(int clienteId)
            => Task.FromResult<IEnumerable<Ordem>>(Itens.Where(o => o.ClienteId == clienteId).ToList());

        public Task<Ordem?> ObterPorIdAsync(int id)
            => Task.FromResult(Itens.FirstOrDefault(o => o.Id == id));

        public Task AdicionarAsync(Ordem ordemServico) => throw new NotImplementedException();

        public Task AtualizarAsync(Ordem ordemServico)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    private sealed class FakeCatalogoRepository : ICatalogoRepository
    {
        public List<CatalogoItem> Itens { get; } = new();

        public Task<CatalogoItem?> ObterPorIdAsync(int id)
            => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

        public Task<IEnumerable<CatalogoItem>> ObterTodosAsync() => throw new NotImplementedException();
        public Task AdicionarAsync(CatalogoItem catalogo) => throw new NotImplementedException();
        public Task AtualizarAsync(CatalogoItem catalogo) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    private sealed class FakeEstoqueRepository : IEstoqueRepository
    {
        public List<Estoque> Itens { get; } = new();

        public Task<Estoque?> ObterPorCatalogoIdAsync(int catalogoId)
            => Task.FromResult(Itens.FirstOrDefault(e => e.CatalogoId == catalogoId));

        public Task<IEnumerable<Estoque>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Estoque?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task AdicionarAsync(Estoque estoque) => throw new NotImplementedException();
        public Task AtualizarAsync(Estoque estoque) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }

    // Repositórios que estes cenários não utilizam
    private sealed class NaoUsado : IClienteRepository, IVeiculoRepository, IServicoRepository
    {
        Task<IEnumerable<Cliente>> IClienteRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<Cliente?> IClienteRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<Cliente?> ObterPorDocumentoAsync(string numero) => throw new NotImplementedException();
        public Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Cliente cliente) => throw new NotImplementedException();
        public Task AtualizarAsync(Cliente cliente) => throw new NotImplementedException();

        Task<IEnumerable<Veiculo>> IVeiculoRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<Veiculo?> IVeiculoRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<Veiculo?> ObterPorPlacaAsync(string placa) => throw new NotImplementedException();
        public Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Veiculo veiculo) => throw new NotImplementedException();
        public Task AtualizarAsync(Veiculo veiculo) => throw new NotImplementedException();

        Task<IEnumerable<Servico>> IServicoRepository.ObterTodosAsync() => throw new NotImplementedException();
        Task<Servico?> IServicoRepository.ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task AdicionarAsync(Servico servico) => throw new NotImplementedException();
        public Task AtualizarAsync(Servico servico) => throw new NotImplementedException();

        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }
    #endregion
}
