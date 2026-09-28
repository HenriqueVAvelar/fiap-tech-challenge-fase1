using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class EstoqueDto
{
    [Required]
    public int CatalogoId { get; set; }

    [Required]
    public int Quantidade { get; set; }
}
