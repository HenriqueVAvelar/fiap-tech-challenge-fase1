using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class ClienteInputDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos), com ou sem máscara.</summary>
    [Required]
    public string Documento { get; set; } = string.Empty;
}
