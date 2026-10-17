using Oficina.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.Usuario;

public class UsuarioUpdateInputDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    public string? Senha { get; set; }

    [Required]
    [EnumDataType(typeof(EnumsUsuario.PerfilUsuario))]
    public EnumsUsuario.PerfilUsuario? Perfil { get; set; }
}
