using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs.Auth;

public class LoginInputDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}
