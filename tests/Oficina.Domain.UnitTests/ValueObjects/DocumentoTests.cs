using Oficina.Domain.Enums;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.UnitTests.ValueObjects;

public class DocumentoTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData(" 529 982 247 25 ", "52998224725")]
    public void DeveAceitarCpfValido_ComOuSemMascara(string entrada, string esperado)
    {
        var documento = new Documento(entrada);

        Assert.Equal(esperado, documento.Numero);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cpf, documento.Tipo);
    }

    [Theory]
    [InlineData("11.444.777/0001-61", "11444777000161")]
    [InlineData("11444777000161", "11444777000161")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    public void DeveAceitarCnpjValido_ComOuSemMascara(string entrada, string esperado)
    {
        var documento = new Documento(entrada);

        Assert.Equal(esperado, documento.Numero);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cnpj, documento.Tipo);
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("52998224720")]
    [InlineData("11.444.777/0001-60")]
    [InlineData("11444777000162")]
    public void DeveLancarExcecao_QuandoDigitosVerificadoresInvalidos(string entrada)
    {
        Assert.Throws<ArgumentException>(() => new Documento(entrada));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000")]
    [InlineData("11.111.111/1111-11")]
    [InlineData("00000000000000")]
    public void DeveLancarExcecao_QuandoTodosOsDigitosSaoIguais(string entrada)
    {
        Assert.Throws<ArgumentException>(() => new Documento(entrada));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("123456789012345")]
    public void DeveLancarExcecao_QuandoVazioOuTamanhoInvalido(string entrada)
    {
        Assert.Throws<ArgumentException>(() => new Documento(entrada));
    }

    [Fact]
    public void Formatado_DeveAplicarMascara()
    {
        Assert.Equal("529.982.247-25", new Documento("52998224725").Formatado());
        Assert.Equal("11.444.777/0001-61", new Documento("11444777000161").Formatado());
    }

    [Fact]
    public void Documentos_ComMesmoNumero_DevemSerIguais()
    {
        Assert.Equal(new Documento("529.982.247-25"), new Documento("52998224725"));
        Assert.NotEqual(new Documento("52998224725"), new Documento("11444777000161"));
    }
}
