using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs.Usuario;

public class UsuarioDto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string UserName { get; set; }
    public EnumsUsuario.PerfilUsuario Perfil { get; set; }
}
