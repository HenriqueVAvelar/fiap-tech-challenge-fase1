using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;
using Oficina.Application.Services;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.UnitTests.Auth;

public class AuthServiceCredenciaisTests
{
    private readonly FakeTokenService _tokens = new();
    private readonly AuthService _service;

    public AuthServiceCredenciaisTests()
    {
        var maria = new Domain.Entities.Usuario("Maria", "maria", "hash:Senha@123", EnumsUsuario.PerfilUsuario.Administrador);
        var joao = new Domain.Entities.Usuario("João", "joao", "hash:Outra#456", EnumsUsuario.PerfilUsuario.Mecanico);

        _service = new AuthService(new FakeRepository(maria, joao), new FakeHasher(), _tokens);
    }

    [Fact]
    public async Task Login_DeveGerarOTokenDoUsuarioInformado()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "joao", Senha = "Outra#456" });

        Assert.Equal("token-de-joao", resultado!.Token);
        Assert.Equal(new[] { "joao" }, _tokens.UsuariosAutenticados);
    }

    [Fact]
    public async Task Login_DeveIgnorarEspacosNoUserName()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "  maria  ", Senha = "Senha@123" });

        Assert.Equal("token-de-maria", resultado!.Token);
    }

    [Fact]
    public async Task Login_DeveRetornarNull_QuandoASenhaEDeOutroUsuario()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "maria", Senha = "Outra#456" });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Login_NaoDeveGerarToken_QuandoCredenciaisInvalidas()
    {
        await _service.LoginAsync(new LoginInputDto { UserName = "maria", Senha = "errada@1" });
        await _service.LoginAsync(new LoginInputDto { UserName = "ninguem", Senha = "Senha@123" });

        Assert.Empty(_tokens.UsuariosAutenticados);
    }

    [Fact]
    public async Task Login_DeveRetornarNull_QuandoASenhaEstaVazia()
    {
        var resultado = await _service.LoginAsync(new LoginInputDto { UserName = "maria", Senha = "" });

        Assert.Null(resultado);
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == Hash(senhaInformada);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public List<string> UsuariosAutenticados { get; } = new();

        public LoginResultDto GerarToken(Domain.Entities.Usuario usuario)
        {
            UsuariosAutenticados.Add(usuario.UserName);
            return new() { Token = $"token-de-{usuario.UserName}", ExpiraEm = DateTime.UtcNow.AddHours(1) };
        }
    }

    private sealed class FakeRepository : IUsuarioRepository
    {
        private readonly List<Domain.Entities.Usuario> _usuarios;

        public FakeRepository(params Domain.Entities.Usuario[] usuarios) => _usuarios = usuarios.ToList();

        public Task<Domain.Entities.Usuario?> ObterPorUserNameAsync(string userName)
            => Task.FromResult(_usuarios.FirstOrDefault(u =>
                string.Equals(u.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<IEnumerable<Domain.Entities.Usuario>> ObterTodosAsync() => throw new NotImplementedException();
        public Task<Domain.Entities.Usuario?> ObterPorIdAsync(int id) => throw new NotImplementedException();
        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Domain.Entities.Usuario usuario) => throw new NotImplementedException();
        public Task AtualizarAsync(Domain.Entities.Usuario usuario) => throw new NotImplementedException();
        public Task RemoverAsync(int id) => throw new NotImplementedException();
    }
}
