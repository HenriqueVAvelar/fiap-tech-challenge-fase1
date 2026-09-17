using System.ComponentModel;

namespace Oficina.Domain.Enums;

public class EnumsConsumivel
{
    public enum TipoConsumivel
    {
        [Description("Peça")]
        Peca = 1,
        [Description("Insumo")]
        Insumo = 2
    }
}
