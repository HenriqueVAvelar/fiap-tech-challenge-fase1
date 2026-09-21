using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class ClienteDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Documento { get; set; } = string.Empty;
}
