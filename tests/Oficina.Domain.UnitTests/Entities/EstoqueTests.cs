using Oficina.Domain.Entities;

namespace Oficina.Domain.UnitTests.Entities;

public class EstoqueTests
{
    [Fact]
    public void RegistrarEntrada_DeveAumentarAQuantidade()
    {
        var estoque = new Estoque(10);

        estoque.RegistrarEntrada(5);

        Assert.Equal(15, estoque.Quantidade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarEntrada_DeveLancarExcecao_QuandoAQuantidadeNaoForPositiva(int quantidade)
    {
        var estoque = new Estoque(10);

        Assert.Throws<ArgumentOutOfRangeException>(() => estoque.RegistrarEntrada(quantidade));
    }

    [Fact]
    public void Consumir_DeveReduzirAQuantidade()
    {
        var estoque = new Estoque(10);

        estoque.Consumir(3);

        Assert.Equal(7, estoque.Quantidade);
    }

    [Fact]
    public void Consumir_DevePermitirZerarOEstoque()
    {
        var estoque = new Estoque(10);

        estoque.Consumir(10);

        Assert.Equal(0, estoque.Quantidade);
    }

    [Fact]
    public void Consumir_DeveLancarExcecao_QuandoAQuantidadeForMaiorQueOEstoque()
    {
        var estoque = new Estoque(10);

        Assert.Throws<InvalidOperationException>(() => estoque.Consumir(11));
    }

    [Fact]
    public void Consumir_NaoDeveAlterarAQuantidade_QuandoAOperacaoFalhar()
    {
        var estoque = new Estoque(10);

        Assert.Throws<InvalidOperationException>(() => estoque.Consumir(11));

        Assert.Equal(10, estoque.Quantidade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Consumir_DeveLancarExcecao_QuandoAQuantidadeNaoForPositiva(int quantidade)
    {
        var estoque = new Estoque(10);

        Assert.Throws<ArgumentOutOfRangeException>(() => estoque.Consumir(quantidade));
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoAQuantidadeInicialForNegativa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Estoque(-1));
    }
}
