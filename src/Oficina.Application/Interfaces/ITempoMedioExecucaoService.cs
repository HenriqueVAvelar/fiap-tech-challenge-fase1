using Oficina.Application.DTOs.Indicadores;

namespace Oficina.Application.Interfaces;

public interface ITempoMedioExecucaoService
{
    Task<TempoMedioExecucaoDto> ObterAsync();
}