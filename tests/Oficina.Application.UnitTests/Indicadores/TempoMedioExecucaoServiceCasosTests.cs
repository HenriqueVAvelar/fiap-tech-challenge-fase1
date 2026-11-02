using Oficina.Application.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Application.UnitTests.Indicadores;

public class TempoMedioExecucaoServiceCasosTests
{
    [Fact]
    public async Task ObterAsync_SemNenhumaOrdem_DeveRetornarTempoNulo()
    {
        var service = new TempoMedioExecucaoService(new RepositoryFake());

        var resultado = await service.ObterAsync();

        Assert.Equal(0, resultado.QuantidadeOrdensConsideradas);
        Assert.Null(resultado.TempoMedioMinutos);
    }

    [Fact]
    public async Task ObterAsync_DeveConsiderarOrdemEntregue()
    {
        var ordem = CriarOrdemEmExecucao();
        ordem.Finalizar();
        ordem.Entregar();
        var service = new TempoMedioExecucaoService(new RepositoryFake(ordem));

        var resultado = await service.ObterAsync();

        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.Entregue, ordem.Status);
        Assert.Equal(1, resultado.QuantidadeOrdensConsideradas);
        Assert.NotNull(resultado.TempoMedioMinutos);
    }

    [Fact]
    public async Task ObterAsync_DeveConsiderarOrdemFinalizadaEEntregueJuntas()
    {
        var finalizada = CriarOrdemEmExecucao();
        finalizada.Finalizar();
        var entregue = CriarOrdemEmExecucao();
        entregue.Finalizar();
        entregue.Entregar();
        var service = new TempoMedioExecucaoService(new RepositoryFake(finalizada, entregue));

        var resultado = await service.ObterAsync();

        Assert.Equal(2, resultado.QuantidadeOrdensConsideradas);
    }

    [Fact]
    public async Task ObterAsync_DeveIgnorarOrdensQueNaoIniciaramAExecucao()
    {
        var recebida = new Ordem(1, 1);
        var emDiagnostico = new Ordem(2, 2);
        emDiagnostico.IniciarDiagnostico();
        var aguardando = CriarOrdemAguardandoAprovacao();
        var service = new TempoMedioExecucaoService(new RepositoryFake(recebida, emDiagnostico, aguardando));

        var resultado = await service.ObterAsync();

        Assert.Equal(0, resultado.QuantidadeOrdensConsideradas);
        Assert.Null(resultado.TempoMedioMinutos);
    }

    [Fact]
    public async Task ObterAsync_NaoDeveRetornarTempoNegativo()
    {
        var ordem = CriarOrdemEmExecucao();
        ordem.Finalizar();
        var service = new TempoMedioExecucaoService(new RepositoryFake(ordem));

        var resultado = await service.ObterAsync();

        Assert.True(resultado.TempoMedioMinutos >= 0);
    }

    private static Ordem CriarOrdemAguardandoAprovacao()
    {
        var ordem = new Ordem(1, 1);

        ordem.AdicionarServico(1, "Troca de óleo", 100m);
        ordem.IniciarDiagnostico();
        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        ordem.DisponibilizarOrcamento();

        return ordem;
    }

    private static Ordem CriarOrdemEmExecucao()
    {
        var ordem = CriarOrdemAguardandoAprovacao();

        ordem.AprovarOrcamento();
        ordem.IniciarExecucao();

        return ordem;
    }

    private sealed class RepositoryFake : IOrdemServicoRepository
    {
        private readonly List<Ordem> _ordens;

        public RepositoryFake(params Ordem[] ordens) => _ordens = ordens.ToList();

        public Task<IEnumerable<Ordem>> ObterTodosAsync()
            => Task.FromResult<IEnumerable<Ordem>>(_ordens.ToList());

        public Task<Ordem?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(int clienteId) => throw new NotImplementedException();
        public Task AdicionarAsync(Ordem ordemServico) => throw new NotImplementedException();
        public Task AtualizarAsync(Ordem ordemServico) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }
}
