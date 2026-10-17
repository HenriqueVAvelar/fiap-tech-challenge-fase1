using Oficina.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.Usuario;

public class UsuarioInputDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(EnumsUsuario.PerfilUsuario))]
    public EnumsUsuario.PerfilUsuario? Perfil { get; set; }
}
