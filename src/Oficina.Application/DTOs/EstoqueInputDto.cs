using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class EstoqueInputDto
{
    [Required]
    public int CatalogoId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantidade { get; set; }
}
