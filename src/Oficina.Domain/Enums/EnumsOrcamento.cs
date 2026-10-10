using System.ComponentModel;

namespace Oficina.Domain.Enums;

public class EnumsOrcamento
{
    public enum StatusOrcamento
    {
        [Description("Pendente")]
        Pendente = 1,
        [Description("Aprovado")]
        Aprovado = 2,
        [Description("Rejeitado")]
        Rejeitado = 3
    }
}