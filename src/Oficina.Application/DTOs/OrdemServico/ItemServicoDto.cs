namespace Oficina.Application.DTOs.OrdemServico;

public class ItemServicoDto
{
    public int ServicoId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }
}