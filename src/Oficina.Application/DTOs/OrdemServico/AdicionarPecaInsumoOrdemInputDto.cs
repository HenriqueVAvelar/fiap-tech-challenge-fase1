using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.OrdemServico;

public class AdicionarPecaInsumoOrdemInputDto
{
    [Required]
    public int CatalogoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }
}