using System.ComponentModel;

namespace Oficina.Domain.Enums;

public class EnumsDocumento
{
    public enum TipoDocumento
    {
        [Description("CPF")]
        Cpf = 1,
        [Description("CNPJ")]
        Cnpj = 2

    }
}
