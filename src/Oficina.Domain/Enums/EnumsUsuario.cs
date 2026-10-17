using System.ComponentModel;

namespace Oficina.Domain.Enums;

public class EnumsUsuario
{
    public enum PerfilUsuario : byte
    {
        [Description("Administrador")]
        Administrador = 1,
        [Description("Mecânico")]
        Mecanico = 2
    }
}
