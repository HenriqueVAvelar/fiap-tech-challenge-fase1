using Oficina.Application.DTOs.Auth;

namespace Oficina.Application.Interfaces;

public interface IAuthService
{
    /// <summary>Retorna o token, ou null quando UserName ou senha são inválidos.</summary>
    Task<LoginResultDto?> LoginAsync(LoginInputDto login);
}
