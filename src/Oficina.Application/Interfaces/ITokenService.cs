using Oficina.Application.DTOs.Auth;
using Oficina.Domain.Entities;

namespace Oficina.Application.Interfaces;

public interface ITokenService
{
    LoginResultDto GerarToken(Usuario usuario);
}
