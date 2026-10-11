using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;
using Oficina.Application.Services;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly AuthService _service;

    public AuthServiceTests()   
    {
        var usuario = new Domain.Entities.Usuario("Maria", "maria", "hash:Senha@123", EnumsUsuario.PerfilUsuario.Administrador);
        _service = new AuthService(new FakeRepository(usuario), new FakeHasher(), new FakeTokenService());
    }

    [Fact]
    public async Task Login_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "MARIA", Senha = "Senha@123" });

        Assert.NotNull(resultado);
        Assert.Equal("token-de-maria", resultado!.Token);
    }

    [Fact]
    public async Task Login_DeveRetornarNull_QuandoSenhaIncorreta()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "maria", Senha = "errada@1" });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Login_DeveRetornarNull_QuandoUsuarioNaoExiste()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "ninguem", Senha = "Senha@123" });

        Assert.Null(resultado);
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == Hash(senhaInformada);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public LoginResultDto GerarToken(Domain.Entities.Usuario usuario)
            => new() { Token = $"token-de-{usuario.UserName}", ExpiraEm = DateTime.UtcNow.AddHours(1) };
    }

    private sealed class FakeRepository : IUsuarioRepository
    {
        private readonly Domain.Entities.Usuario _usuario;

        public FakeRepository(Domain.Entities.Usuario usuario) => _usuario = usuario;

        public Task<Domain.Entities.Usuario?> ObterPorUserNameAsync(string userName)
            => Task.FromResult<Domain.Entities.Usuario?>(
                string.Equals(_usuario.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase) ? _usuario : null);

        public Task<IEnumerable<Domain.Entities.Usuario>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Domain.Entities.Usuario?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Domain.Entities.Usuario usuario) => throw new NotImplementedException();
        public Task AtualizarAsync(Domain.Entities.Usuario usuario) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }
}
