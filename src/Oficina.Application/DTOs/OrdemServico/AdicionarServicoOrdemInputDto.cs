using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.OrdemServico;

public class AdicionarServicoOrdemInputDto
{
    [Required]
    public int ServicoId { get; set; }
}