using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.OrdemServico;

public class RegistrarConsumoOrdemInputDto
{    
    [Required]
    [MinLength(1)]
    public List<int> ItensIds { get; set; } = new();
}
