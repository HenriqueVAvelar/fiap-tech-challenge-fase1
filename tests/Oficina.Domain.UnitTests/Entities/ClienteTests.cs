using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.UnitTests.Entities;

public class ClienteTests
{
    private static Documento CpfValido(string valor = "529.982.247-25") => new(valor);

    [Fact]
    public void Construtor_DeveAtribuirNomeSemEspacos_EDocumento()
    {
        var cliente = new Cliente("  Maria  ", CpfValido());

        Assert.Equal("Maria", cliente.Nome);
        Assert.Equal("52998224725", cliente.Documento.Numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeVazio(string nome)
    {
        Assert.Throws<ArgumentException>(() => new Cliente(nome, CpfValido()));
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoDocumentoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => new Cliente("Maria", null!));
    }

    [Fact]
    public void Atualizar_DeveAlterarNomeEDocumento()
    {
        var cliente = new Cliente("Maria", CpfValido());

        cliente.Atualizar(" João ", CpfValido("111.444.777-35"));

        Assert.Equal("João", cliente.Nome);
        Assert.Equal("11144477735", cliente.Documento.Numero);
    }

    [Fact]
    public void Atualizar_DeveLancarExcecao_QuandoDadosInvalidos()
    {
        var cliente = new Cliente("Maria", CpfValido());

        Assert.Throws<ArgumentException>(() => cliente.Atualizar("", CpfValido()));
        Assert.Throws<ArgumentNullException>(() => cliente.Atualizar("João", null!));
        Assert.Equal("Maria", cliente.Nome);
    }
}
