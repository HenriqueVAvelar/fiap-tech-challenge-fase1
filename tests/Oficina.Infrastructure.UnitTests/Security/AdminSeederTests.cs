using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Security;
using Oficina.Infrastructure.UnitTests.Support;

namespace Oficina.Infrastructure.UnitTests.Security;

public class AdminSeederTests
{
    private readonly FakeUsuarioRepository _repository = new();
    private readonly LoggerFactoryCapture _logs = new();

    private IServiceProvider CriarServicos(AdminSeedSettings settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<AdminSeedSettings>>(Options.Create(settings));
        services.AddSingleton<ILoggerFactory>(_logs);
        services.AddSingleton<IUsuarioRepository>(_repository);
        services.AddSingleton<IPasswordHasher, FakeHasher>();

        return services.BuildServiceProvider();
    }

    private static AdminSeedSettings Configuracao(
        string nome = "Chefe",
        string userName = "admin",
        string senha = "Senha@123")
        => new() { Nome = nome, UserName = userName, Senha = senha };

    [Fact]
    public async Task SeedAdmin_DeveCriarOAdministrador_ComSenhaHasheada()
    {
        await CriarServicos(Configuracao()).SeedAdminAsync();

        var admin = Assert.Single(_repository.Usuarios);
        Assert.Equal("Chefe", admin.Nome);
        Assert.Equal("admin", admin.UserName);
        Assert.Equal("hash:Senha@123", admin.SenhaHash);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Administrador, admin.Perfil);
        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Information);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SeedAdmin_DeveUsarNomePadrao_QuandoNomeNaoInformado(string nome)
    {
        await CriarServicos(Configuracao(nome: nome)).SeedAdminAsync();

        Assert.Equal("Administrador", Assert.Single(_repository.Usuarios).Nome);
    }

    [Theory]
    [InlineData("", "Senha@123")]
    [InlineData("   ", "Senha@123")]
    [InlineData("admin", "")]
    [InlineData("admin", "   ")]
    public async Task SeedAdmin_NaoDeveFazerNada_QuandoConfiguracaoIncompleta(string userName, string senha)
    {
        await CriarServicos(Configuracao(userName: userName, senha: senha)).SeedAdminAsync();

        Assert.Empty(_repository.Usuarios);
        Assert.Empty(_logs.Entradas);
    }

    [Fact]
    public async Task SeedAdmin_NaoDeveCriarOutro_QuandoJaExisteAdministrador()
    {
        _repository.Usuarios.Add(new Usuario("Existente", "existente", "hash", EnumsUsuario.PerfilUsuario.Administrador));

        await CriarServicos(Configuracao()).SeedAdminAsync();

        Assert.Equal("existente", Assert.Single(_repository.Usuarios).UserName);
    }

    [Fact]
    public async Task SeedAdmin_DeveCriarOAdministrador_MesmoQueExistaApenasMecanico()
    {
        _repository.Usuarios.Add(new Usuario("Joao", "joao", "hash", EnumsUsuario.PerfilUsuario.Mecanico));

        await CriarServicos(Configuracao()).SeedAdminAsync();

        Assert.Equal(2, _repository.Usuarios.Count);
        Assert.Contains(_repository.Usuarios, u => u.UserName == "admin");
    }

    [Fact]
    public async Task SeedAdmin_DeveIgnorarEAvisar_QuandoOUserNameJaEstaEmUso()
    {
        _repository.Usuarios.Add(new Usuario("Joao", "admin", "hash", EnumsUsuario.PerfilUsuario.Mecanico));

        await CriarServicos(Configuracao()).SeedAdminAsync();

        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, Assert.Single(_repository.Usuarios).Perfil);
        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Warning);
    }

    [Fact]
    public async Task SeedAdmin_NaoDeveLancarExcecao_QuandoASenhaNaoTemCaractereEspecial()
    {
        await CriarServicos(Configuracao(senha: "SemEspecial1")).SeedAdminAsync();

        Assert.Empty(_repository.Usuarios);
        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Error && e.Excecao is ArgumentException);
    }

    [Fact]
    public async Task SeedAdmin_NaoDeveLancarExcecao_QuandoOBancoEstaIndisponivel()
    {
        _repository.FalharAoObterTodos = true;

        await CriarServicos(Configuracao()).SeedAdminAsync();

        Assert.Empty(_repository.Usuarios);
        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Error && e.Excecao is InvalidOperationException);
    }

    [Fact]
    public async Task SeedAdmin_DeveSerIdempotente_QuandoExecutadoDuasVezes()
    {
        var servicos = CriarServicos(Configuracao());

        await servicos.SeedAdminAsync();
        await servicos.SeedAdminAsync();

        Assert.Single(_repository.Usuarios);
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == Hash(senhaInformada);
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();
        public bool FalharAoObterTodos { get; set; }

        public Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            if (FalharAoObterTodos)
                throw new InvalidOperationException("Banco indisponível.");

            return Task.FromResult<IEnumerable<Usuario>>(Usuarios.ToList());
        }

        public Task<Usuario?> ObterPorIdAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));

        public Task<Usuario?> ObterPorUserNameAsync(string userName)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.UserName == userName));

        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null)
            => Task.FromResult(Usuarios.Any(u => u.UserName == userName));

        public Task AdicionarAsync(Usuario usuario)
        {
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Usuario usuario) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Usuarios.RemoveAll(u => u.Id == id);
            return Task.CompletedTask;
        }
    }
}
