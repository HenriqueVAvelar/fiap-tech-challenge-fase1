using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs.OrdemServico;

public class OrdemServicoDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int VeiculoId { get; set; }

    public EnumsOrdemServico.StatusOrdemServico Status { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataInicioDiagnostico { get; set; }

    public DateTime? DataConclusaoDiagnostico { get; set; }

    public DateTime? DataInicioExecucao { get; set; }

    public DateTime? DataFinalizacao { get; set; }

    public DateTime? DataEntrega { get; set; }

    public IEnumerable<ItemServicoDto> ItensServico { get; set; }
        = Enumerable.Empty<ItemServicoDto>();

    public IEnumerable<ItemPecaInsumoDto> ItensPecaInsumo { get; set; }
        = Enumerable.Empty<ItemPecaInsumoDto>();

    public OrcamentoDto? Orcamento { get; set; }
}