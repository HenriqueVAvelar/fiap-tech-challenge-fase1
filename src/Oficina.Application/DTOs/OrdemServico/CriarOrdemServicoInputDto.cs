using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.OrdemServico;

public class CriarOrdemServicoInputDto
{
    [Required]
    public string Documento { get; set; } = string.Empty;

    [Required]
    public string Placa { get; set; } = string.Empty;
}