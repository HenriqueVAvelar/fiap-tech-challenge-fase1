using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs.OrdemServico;

public class ItemPecaInsumoDto
{
    public int Id { get; set; }

    public int CatalogoId { get; set; }

    public bool Consumido { get; set; }

    public string Nome { get; set; } = string.Empty;

    public EnumsConsumivel.TipoConsumivel Tipo { get; set; }

    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal ValorTotal { get; set; }
}