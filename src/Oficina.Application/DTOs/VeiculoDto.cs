using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;
public class VeiculoDto
{
    [Required]
    public int ClienteId { get; set; }
    [Required]
    public string Placa { get; set; } = string.Empty;

    [Required]
    public string Marca { get; set; } = string.Empty;

    [Required]
    public string Modelo { get; set; } = string.Empty;

    [Required]
    public int Ano { get; set; }
}
