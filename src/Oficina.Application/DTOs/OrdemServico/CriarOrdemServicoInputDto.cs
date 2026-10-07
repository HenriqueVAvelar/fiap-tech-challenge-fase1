using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.OrdemServico;

public class CriarOrdemServicoInputDto
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int VeiculoId { get; set; }
}