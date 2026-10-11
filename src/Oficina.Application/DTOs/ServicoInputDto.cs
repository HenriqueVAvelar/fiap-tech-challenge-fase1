using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class ServicoInputDto
{
    [Required]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public bool Ativo { get; set; }
}
