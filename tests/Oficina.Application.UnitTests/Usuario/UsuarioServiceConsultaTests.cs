using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs.Usuario;
using Oficina.Application.Interfaces;
using Oficina.Application.Mappings;
using Oficina.Application.Services;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.UnitTests.Usuario;

public class UsuarioServiceConsultaTests
{
    private readonly FakeUsuarioRepository _repository = new();
    private readonly UsuarioService _service;

    public UsuarioServiceConsultaTests()
    {
        _service = new UsuarioService(_repository, new FakeHasher(), CriarMapper());
    }

    private static IMapper CriarMapper()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<UsuarioProfile>();
        return new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();
    }

    private static UsuarioInputDto Input(string userName, EnumsUsuario.PerfilUsuario perfil = EnumsUsuario.PerfilUsuario.Administrador)
        => new()
        {
            Nome = $"Nome {userName}",
            UserName = userName,
            Senha = "Senha@123",
            Perfil = perfil
        };

    [Fact]
    public async Task ObterTodos_DeveListarOsUsuarios_SemExporASenha()
    {
        await _service.CriarAsync(Input("admin"));
        await _service.CriarAsync(Input("mecanico", EnumsUsuario.PerfilUsuario.Mecanico));

        var resultado = (await _service.ObterTodosAsync()).ToList();

        Assert.Equal(new[] { "admin", "mecanico" }, resultado.Select(u => u.UserName));
        Assert.Equal(
            new[] { EnumsUsuario.PerfilUsuario.Administrador, EnumsUsuario.PerfilUsuario.Mecanico },
            resultado.Select(u => u.Perfil));
        Assert.DoesNotContain(typeof(UsuarioDto).GetProperties(), p => p.Name.Contains("Senha"));
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarListaVazia_QuandoNaoHaUsuarios()
    {
        Assert.Empty(await _service.ObterTodosAsync());
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarOUsuario_QuandoExiste()
    {
        var criado = await _service.CriarAsync(Input("admin"));

        var resultado = await _service.ObterPorIdAsync(criado.Id);

        Assert.NotNull(resultado);
        Assert.Equal(criado.Id, resultado!.Id);
        Assert.Equal("Nome admin", resultado.Nome);
        Assert.Equal("admin", resultado.UserName);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNull_QuandoNaoExiste()
    {
        Assert.Null(await _service.ObterPorIdAsync(42));
    }

    [Fact]
    public async Task Criar_DeveRemoverEspacosDoNomeEDoUserName()
    {
        var dto = await _service.CriarAsync(new UsuarioInputDto
        {
            Nome = "  Maria  ",
            UserName = "  maria  ",
            Senha = "Senha@123",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        Assert.Equal("Maria", dto.Nome);
        Assert.Equal("maria", dto.UserName);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoUserNameJaExisteComEspacos()
    {
        await _service.CriarAsync(Input("admin"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarAsync(Input("  admin  ")));
    }

    [Fact]
    public async Task Criar_NaoDevePersistir_QuandoSenhaInvalida()
    {
        var input = Input("admin");
        input.Senha = "semespecial1";

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(input));

        Assert.Empty(_repository.Usuarios);
    }

    [Fact]
    public async Task Atualizar_DeveAlterarNomeUserNameEPerfil()
    {
        var criado = await _service.CriarAsync(Input("admin"));

        var ok = await _service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = " Novo nome ",
            UserName = " novo ",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        Assert.True(ok);
        var salvo = _repository.Usuarios.Single();
        Assert.Equal("Novo nome", salvo.Nome);
        Assert.Equal("novo", salvo.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, salvo.Perfil);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Atualizar_DeveManterASenha_QuandoNaoInformada(string? senha)
    {
        var criado = await _service.CriarAsync(Input("admin"));

        await _service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = "Administrador",
            UserName = "admin",
            Senha = senha,
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        });

        Assert.Equal("hash:Senha@123", _repository.Usuarios.Single().SenhaHash);
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoNovaSenhaNaoTemCaractereEspecial()
    {
        var criado = await _service.CriarAsync(Input("admin"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = "Administrador",
            UserName = "admin",
            Senha = "semespecial1",
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        }));

        Assert.Equal("hash:Senha@123", _repository.Usuarios.Single().SenhaHash);
    }

    [Fact]
    public async Task Remover_NaoDeveAfetarOsDemaisUsuarios()
    {
        var admin = await _service.CriarAsync(Input("admin"));
        await _service.CriarAsync(Input("mecanico", EnumsUsuario.PerfilUsuario.Mecanico));

        Assert.True(await _service.RemoverAsync(admin.Id));

        Assert.Equal("mecanico", Assert.Single(_repository.Usuarios).UserName);
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == Hash(senhaInformada);
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private int _proximoId = 1;
        public List<Domain.Entities.Usuario> Usuarios { get; } = new();

        public Task<IEnumerable<Domain.Entities.Usuario>> ObterTodosAsync()
            => Task.FromResult<IEnumerable<Domain.Entities.Usuario>>(Usuarios.ToList());

        public Task<Domain.Entities.Usuario?> ObterPorIdAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));

        public Task<Domain.Entities.Usuario?> ObterPorUserNameAsync(string userName)
            => Task.FromResult(Usuarios.FirstOrDefault(u =>
                string.Equals(u.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null)
            => Task.FromResult(Usuarios.Any(u =>
                string.Equals(u.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                && (ignorarId == null || u.Id != ignorarId)));

        public Task AdicionarAsync(Domain.Entities.Usuario usuario)
        {
            typeof(Domain.Entities.Usuario).GetProperty(nameof(Domain.Entities.Usuario.Id))!
                .SetValue(usuario, _proximoId++);
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Domain.Entities.Usuario usuario) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Usuarios.RemoveAll(u => u.Id == id);
            return Task.CompletedTask;
        }
    }
}
