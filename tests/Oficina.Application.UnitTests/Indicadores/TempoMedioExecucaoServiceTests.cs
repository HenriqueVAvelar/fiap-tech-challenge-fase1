using Oficina.Application.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Application.UnitTests.Indicadores;

public class TempoMedioExecucaoServiceTests
{
    [Fact]
    public async Task ObterAsync_SemOrdensConcluidas_DeveRetornarTempoNulo()
    {
        var repository =
            new OrdemServicoRepositoryFake(
                new[]
                {
                    new Ordem(1, 1)
                });

        var service =
            new TempoMedioExecucaoService(repository);

        var resultado = await service.ObterAsync();

        Assert.Equal(
            0,
            resultado.QuantidadeOrdensConsideradas);

        Assert.Null(
            resultado.TempoMedioMinutos);
    }

    [Fact]
    public async Task ObterAsync_ComOrdensConcluidas_DeveCalcularMedia()
    {
        var ordem1 =
            CriarOrdemFinalizada(1, 1);

        var ordem2 =
            CriarOrdemFinalizada(2, 2);

        var repository =
            new OrdemServicoRepositoryFake(
                new[]
                {
                    ordem1,
                    ordem2
                });

        var service =
            new TempoMedioExecucaoService(repository);

        var resultado = await service.ObterAsync();

        var mediaEsperada =
            new[]
            {
                (ordem1.DataFinalizacao!.Value -
                 ordem1.DataInicioExecucao!.Value)
                    .TotalMinutes,

                (ordem2.DataFinalizacao!.Value -
                 ordem2.DataInicioExecucao!.Value)
                    .TotalMinutes
            }
            .Average();

        Assert.Equal(
            2,
            resultado.QuantidadeOrdensConsideradas);

        Assert.NotNull(
            resultado.TempoMedioMinutos);

        Assert.Equal(
            mediaEsperada,
            resultado.TempoMedioMinutos!.Value,
            10);
    }

    [Fact]
    public async Task ObterAsync_DeveIgnorarOrdemAindaEmExecucao()
    {
        var ordemFinalizada =
            CriarOrdemFinalizada(1, 1);

        var ordemEmExecucao =
            CriarOrdemEmExecucao(2, 2);

        var repository =
            new OrdemServicoRepositoryFake(
                new[]
                {
                    ordemFinalizada,
                    ordemEmExecucao
                });

        var service =
            new TempoMedioExecucaoService(repository);

        var resultado = await service.ObterAsync();

        Assert.Equal(
            1,
            resultado.QuantidadeOrdensConsideradas);

        Assert.NotNull(
            resultado.TempoMedioMinutos);
    }

    private static Ordem CriarOrdemFinalizada(
        int clienteId,
        int veiculoId)
    {
        var ordem =
            CriarOrdemEmExecucao(
                clienteId,
                veiculoId);

        ordem.Finalizar();

        return ordem;
    }

    private static Ordem CriarOrdemEmExecucao(
        int clienteId,
        int veiculoId)
    {
        var ordem =
            new Ordem(
                clienteId,
                veiculoId);

        ordem.AdicionarServico(
            1,
            "Serviço de teste",
            100m);

        ordem.IniciarDiagnostico();

        ordem.ConcluirDiagnostico();

        ordem.GerarOrcamento();

        ordem.DisponibilizarOrcamento();

        ordem.AprovarOrcamento();

        ordem.IniciarExecucao();

        return ordem;
    }

    private sealed class OrdemServicoRepositoryFake
        : IOrdemServicoRepository
    {
        private readonly IEnumerable<Ordem> _ordens;

        public OrdemServicoRepositoryFake(
            IEnumerable<Ordem> ordens)
        {
            _ordens = ordens;
        }

        public Task<IEnumerable<Ordem>>
            ObterTodosAsync()
        {
            return Task.FromResult(_ordens);
        }

        public Task<Ordem?> ObterPorIdAsync(
            int id)
        {
            return Task.FromResult<Ordem?>(
                null);
        }

        public Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(
            int clienteId)
        {
            return Task.FromResult(
                _ordens.Where(o => o.ClienteId == clienteId));
        }

        public Task AdicionarAsync(
            Ordem ordemServico)
        {
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(
            Ordem ordemServico)
        {
            return Task.CompletedTask;
        }

        public Task RemoverAsync(
            int id)
        {
            return Task.CompletedTask;
        }
    }
}