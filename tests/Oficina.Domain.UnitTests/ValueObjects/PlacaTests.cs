using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.UnitTests.ValueObjects;

public class PlacaTests
{
    [Theory]
    [InlineData("ABC1234", "ABC1234")]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData(" abc 1234 ", "ABC1234")]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData("abc-1d23", "ABC1D23")]
    public void DeveAceitarFormatoAntigoEMercosul_ENormalizar(string entrada, string esperado)
    {
        var placa = new Placa(entrada);

        Assert.Equal(esperado, placa.Valor);
        Assert.Equal(esperado, placa.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecao_QuandoVazia(string? valor)
    {
        Assert.Throws<ArgumentException>(() => new Placa(valor!));
    }

    [Theory]
    [InlineData("AB1234")]
    [InlineData("ABCD123")]
    [InlineData("ABC12345")]
    [InlineData("1234ABC")]
    [InlineData("ABC1DD3")]
    public void DeveLancarExcecao_QuandoFormatoInvalido(string valor)
    {
        Assert.Throws<ArgumentException>(() => new Placa(valor));
    }
}
