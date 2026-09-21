using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs.Catalogo;

public class CatalogoDto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal Preco { get; set; }
    public EnumsConsumivel.TipoConsumivel Tipo { get; set; }
    public bool Ativo { get; set; }
    public string? Descricao { get; set; }
}
