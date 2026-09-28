using Oficina.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.Catalogo;

public class CatalogoInputDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public decimal Preco { get; set; }

    [Required]
    public EnumsConsumivel.TipoConsumivel Tipo { get; set; }

    [Required]
    public bool Ativo { get; set; }

    public string? Descricao { get; set; }
}
