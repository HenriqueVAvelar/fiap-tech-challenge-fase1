using Oficina.Application.DTOs.Indicadores;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class TempoMedioExecucaoService : ITempoMedioExecucaoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public TempoMedioExecucaoService(
        IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<TempoMedioExecucaoDto> ObterAsync()
    {
        var ordens = await _ordemServicoRepository.ObterTodosAsync();

        var ordensConcluidas = ordens
            .Where(ordem =>
                (ordem.Status ==
                    EnumsOrdemServico.StatusOrdemServico.Finalizada ||
                 ordem.Status ==
                    EnumsOrdemServico.StatusOrdemServico.Entregue) &&
                ordem.DataInicioExecucao.HasValue &&
                ordem.DataFinalizacao.HasValue)
            .ToList();

        if (ordensConcluidas.Count == 0)
        {
            return new TempoMedioExecucaoDto
            {
                QuantidadeOrdensConsideradas = 0,
                TempoMedioMinutos = null
            };
        }

        var tempoMedioMinutos = ordensConcluidas
            .Average(ordem =>
                (ordem.DataFinalizacao!.Value -
                 ordem.DataInicioExecucao!.Value)
                .TotalMinutes);

        return new TempoMedioExecucaoDto
        {
            QuantidadeOrdensConsideradas =
                ordensConcluidas.Count,

            TempoMedioMinutos =
                tempoMedioMinutos
        };
    }
}