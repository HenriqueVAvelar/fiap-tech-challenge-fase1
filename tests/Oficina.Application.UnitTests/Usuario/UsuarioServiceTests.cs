using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs.Usuario;
using Oficina.Application.Interfaces;
using Oficina.Application.Mappings;
using Oficina.Application.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.UnitTests.Usuario;

public class UsuarioServiceTests
{
    private readonly FakeUsuarioRepository _repository = new();
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<UsuarioProfile>();
        var mapper = new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();

        _service = new UsuarioService(_repository, new FakeHasher(), mapper);
    }

    private static UsuarioInputDto Input(string userName = "admin", string senha = "Senha@123")
        => new()
        {
            Nome = "Administrador",
            UserName = userName,
            Senha = senha,
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        };

    [Fact]
    public async Task Criar_DevePersistirComSenhaHasheada_ENaoExporSenha()
    {
        var dto = await _service.CriarAsync(Input());

        var salvo = _repository.Usuarios.Single();
        Assert.Equal("hash:Senha@123", salvo.SenhaHash);
        Assert.Equal("admin", dto.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Administrador, dto.Perfil);
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoUserNameJaExiste_IgnorandoCaixa()
    {
        await _service.CriarAsync(Input("admin"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarAsync(Input("ADMIN")));
    }

    [Fact]
    public async Task Criar_DeveLancarExcecao_QuandoSenhaSemCaractereEspecial()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(Input(senha: "Senha123")));
    }

    [Fact]
    public async Task Atualizar_DevePermitirManterOProprioUserName()
    {
        var criado = await _service.CriarAsync(Input());

        var ok = await _service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = "Novo nome",
            UserName = "admin",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        Assert.True(ok);
        var salvo = _repository.Usuarios.Single();
        Assert.Equal("Novo nome", salvo.Nome);
        Assert.Equal("hash:Senha@123", salvo.SenhaHash);
    }

    [Fact]
    public async Task Atualizar_DeveLancarExcecao_QuandoUserNameDeOutroUsuario()
    {
        await _service.CriarAsync(Input("admin"));
        var outro = await _service.CriarAsync(Input("mecanico"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AtualizarAsync(outro.Id, new UsuarioUpdateInputDto
        {
            Nome = "Outro",
            UserName = "admin",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        }));
    }

    [Fact]
    public async Task Atualizar_DeveTrocarSenha_QuandoInformada()
    {
        var criado = await _service.CriarAsync(Input());

        await _service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = "Administrador",
            UserName = "admin",
            Senha = "Nova#Senha",
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        });

        Assert.Equal("hash:Nova#Senha", _repository.Usuarios.Single().SenhaHash);
    }

    [Fact]
    public async Task AtualizarERemover_DevemRetornarFalse_QuandoNaoEncontrado()
    {
        Assert.False(await _service.AtualizarAsync(42, new UsuarioUpdateInputDto
        {
            Nome = "x",
            UserName = "x",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        }));
        Assert.False(await _service.RemoverAsync(42));
    }

    [Fact]
    public async Task Remover_DeveExcluirOUsuario()
    {
        var criado = await _service.CriarAsync(Input());

        Assert.True(await _service.RemoverAsync(criado.Id));
        Assert.Empty(_repository.Usuarios);
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
