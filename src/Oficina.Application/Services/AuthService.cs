using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUsuarioRepository repository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResultDto?> LoginAsync(LoginInputDto login)
    {
        var usuario = await _repository.ObterPorUserNameAsync(login.UserName);

        if (usuario is null || !_passwordHasher.Verificar(usuario.SenhaHash, login.Senha))
            return null;

        return _tokenService.GerarToken(usuario);
    }
}
