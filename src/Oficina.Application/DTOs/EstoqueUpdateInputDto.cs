using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

/// <summary>O item de catálogo de um estoque não muda; só a quantidade pode ser ajustada.</summary>
public class EstoqueUpdateInputDto
{
    [Range(0, int.MaxValue)]
    public int Quantidade { get; set; }
}
