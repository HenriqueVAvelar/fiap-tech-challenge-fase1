using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs.OrdemServico;

public class OrcamentoDto
{
    public decimal ValorServicos { get; set; }

    public decimal ValorPecasInsumos { get; set; }

    public decimal ValorTotal { get; set; }

    public EnumsOrcamento.StatusOrcamento Status { get; set; }

    public DateTime DataGeracao { get; set; }

    public DateTime? DataDisponibilizacao { get; set; }

    public DateTime? DataDecisao { get; set; }
}