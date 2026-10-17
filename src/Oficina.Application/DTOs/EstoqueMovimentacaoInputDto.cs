using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs;

public class EstoqueMovimentacaoInputDto
{
    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }
}
