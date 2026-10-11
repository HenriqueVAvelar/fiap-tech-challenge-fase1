using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.Entities;

public class UsuarioTests
{
    [Theory]
    [InlineData("senha@123")]
    [InlineData("Abc#1234")]
    [InlineData("a!")]
    public void ValidarSenha_DeveAceitar_QuandoContemCaractereEspecial(string senha)
    {
        Usuario.ValidarSenha(senha);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("senha123")]
    [InlineData("senha 123")]
    public void ValidarSenha_DeveLancarExcecao_QuandoSemCaractereEspecial(string senha)
    {
        Assert.Throws<ArgumentException>(() => Usuario.ValidarSenha(senha));
    }

    [Fact]
    public void Criar_DeveAplicarOsDadosInformados()
    {
        var usuario = new Usuario(" Maria ", " maria ", "hash", EnumsUsuario.PerfilUsuario.Mecanico);

        Assert.Equal("Maria", usuario.Nome);
        Assert.Equal("maria", usuario.UserName);
        Assert.Equal("hash", usuario.SenhaHash);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, usuario.Perfil);
    }

    [Theory]
    [InlineData("", "user", "hash")]
    [InlineData("Nome", " ", "hash")]
    [InlineData("Nome", "user", "")]
    public void Criar_DeveLancarExcecao_QuandoCampoObrigatorioInvalido(string nome, string userName, string hash)
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(nome, userName, hash, EnumsUsuario.PerfilUsuario.Administrador));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoPerfilNaoPertenceAoEnum()
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario("Nome", "user", "hash", (EnumsUsuario.PerfilUsuario)99));
    }
}
