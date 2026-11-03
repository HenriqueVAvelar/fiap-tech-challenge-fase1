using Oficina.Domain.Enums;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.UnitTests.ValueObjects;

public class DocumentoIgualdadeTests
{
    [Fact]
    public void ToString_DeveRetornarSomenteOsDigitos()
    {
        Assert.Equal("52998224725", new Documento("529.982.247-25").ToString());
        Assert.Equal("11444777000161", new Documento("11.444.777/0001-61").ToString());
    }

    [Fact]
    public void Equals_Documento_DeveRetornarFalso_QuandoOutroForNulo()
    {
        Assert.False(new Documento("52998224725").Equals((Documento?)null));
    }

    [Fact]
    public void Equals_Object_DeveComparar_PeloNumero()
    {
        var documento = new Documento("52998224725");

        Assert.True(documento.Equals((object)new Documento("529.982.247-25")));
        Assert.False(documento.Equals((object)new Documento("11444777000161")));
        Assert.False(documento.Equals((object)"52998224725"));
        Assert.False(documento.Equals((object?)null));
    }

    [Fact]
    public void GetHashCode_DeveSerIgual_ParaDocumentosComMesmoNumero()
    {
        Assert.Equal(
            new Documento("529.982.247-25").GetHashCode(),
            new Documento("52998224725").GetHashCode());
    }

    [Fact]
    public void DeveAceitarCpf_QuandoPrimeiroDigitoVerificadorForZeroPorResto()
    {
        var documento = new Documento("100.000.001-08");

        Assert.Equal("10000000108", documento.Numero);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cpf, documento.Tipo);
    }

    [Fact]
    public void DeveAceitarCnpj_QuandoPrimeiroDigitoVerificadorForZeroPorResto()
    {
        var documento = new Documento("10.000.008/0001-01");

        Assert.Equal("10000008000101", documento.Numero);
        Assert.Equal(EnumsDocumento.TipoDocumento.Cnpj, documento.Tipo);
    }

    [Fact]
    public void Formatado_DeveManterOsZerosAEsquerda()
    {
        Assert.Equal("100.000.001-08", new Documento("10000000108").Formatado());
        Assert.Equal("10.000.008/0001-01", new Documento("10000008000101").Formatado());
    }

    [Fact]
    public void DeveLancarExcecao_QuandoNaoHouverDigitosNaEntrada()
    {
        Assert.Throws<ArgumentException>(() => new Documento("abc.def.ghi-jk"));
    }
}
