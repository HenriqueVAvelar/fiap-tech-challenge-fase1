using Oficina.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class OrdemServicoDto
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int VeiculoId { get; set; }

    [Required]
    public EnumsOrdemServico.StatusOrdemServico Status { get; set; }

    [Required]
    public string ItensServico { get; set; } = string.Empty;

    [Required]
    public string ItensPecaInsumo { get; set; } = string.Empty;

    [Required]
    public decimal Orcamento { get; set; }
}
