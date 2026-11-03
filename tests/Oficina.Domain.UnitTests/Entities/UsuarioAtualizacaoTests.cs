using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.Entities;

public class UsuarioAtualizacaoTests
{
    private static Usuario CriarUsuario() =>
        new("Maria", "maria", "hash", EnumsUsuario.PerfilUsuario.Mecanico);

    [Fact]
    public void Atualizar_DeveAlterarOsDados_SemAlterarASenha()
    {
        var usuario = CriarUsuario();

        usuario.Atualizar(" João ", " joao ", EnumsUsuario.PerfilUsuario.Administrador);

        Assert.Equal("João", usuario.Nome);
        Assert.Equal("joao", usuario.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Administrador, usuario.Perfil);
        Assert.Equal("hash", usuario.SenhaHash);
    }

    [Theory]
    [InlineData("", "user")]
    [InlineData("   ", "user")]
    [InlineData("Nome", "")]
    [InlineData("Nome", "   ")]
    public void Atualizar_DeveLancarExcecao_QuandoCampoObrigatorioInvalido(string nome, string userName)
    {
        var usuario = CriarUsuario();

        Assert.Throws<ArgumentException>(() =>
            usuario.Atualizar(nome, userName, EnumsUsuario.PerfilUsuario.Administrador));

        Assert.Equal("Maria", usuario.Nome);
        Assert.Equal("maria", usuario.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, usuario.Perfil);
    }

    [Fact]
    public void Atualizar_DeveLancarExcecao_QuandoPerfilNaoPertenceAoEnum()
    {
        var usuario = CriarUsuario();

        Assert.Throws<ArgumentException>(() =>
            usuario.Atualizar("Nome", "user", (EnumsUsuario.PerfilUsuario)99));

        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, usuario.Perfil);
    }

    [Fact]
    public void AlterarSenha_DeveSubstituirOHash()
    {
        var usuario = CriarUsuario();

        usuario.AlterarSenha("novo-hash");

        Assert.Equal("novo-hash", usuario.SenhaHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarSenha_DeveLancarExcecao_QuandoHashVazio(string senhaHash)
    {
        var usuario = CriarUsuario();

        Assert.Throws<ArgumentException>(() => usuario.AlterarSenha(senhaHash));

        Assert.Equal("hash", usuario.SenhaHash);
    }

    [Fact]
    public void ValidarSenha_DeveLancarExcecao_QuandoSenhaForNula()
    {
        Assert.Throws<ArgumentException>(() => Usuario.ValidarSenha(null!));
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("abcdefgh")]
    [InlineData("Senha 123 ")]
    public void ValidarSenha_DeveLancarExcecao_QuandoSomenteLetrasDigitosOuEspacos(string senha)
    {
        Assert.Throws<ArgumentException>(() => Usuario.ValidarSenha(senha));
    }

    [Theory]
    [InlineData("senha_123")]
    [InlineData("senha-123")]
    [InlineData("çenha@")]
    public void ValidarSenha_DeveAceitar_QuandoContemOutrosCaracteresEspeciais(string senha)
    {
        Usuario.ValidarSenha(senha);
    }
}
