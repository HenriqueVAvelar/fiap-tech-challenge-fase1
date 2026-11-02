using Oficina.Domain.Entities;

namespace Oficina.Domain.UnitTests.Entities;

public class EstoqueQuantidadeInicialTests
{
    [Fact]
    public void Construtor_DeveIniciarComQuantidadeZero_QuandoNaoInformada()
    {
        var estoque = new Estoque();

        Assert.Equal(0, estoque.Quantidade);
    }

    [Fact]
    public void Construtor_DeveAceitarQuantidadeInicialZero()
    {
        var estoque = new Estoque(0);

        Assert.Equal(0, estoque.Quantidade);
        Assert.False(estoque.PossuiSaldoPara(1));
    }

    [Fact]
    public void Consumir_DeveLancarExcecao_QuandoEstoqueEstiverZerado()
    {
        var estoque = new Estoque();

        Assert.Throws<InvalidOperationException>(() => estoque.Consumir(1));
    }

    [Fact]
    public void RegistrarEntrada_EConsumir_DevemManterOSaldoCorreto()
    {
        var estoque = new Estoque(2);

        estoque.RegistrarEntrada(8);
        estoque.Consumir(4);

        Assert.Equal(6, estoque.Quantidade);
        Assert.True(estoque.PossuiSaldoPara(6));
        Assert.False(estoque.PossuiSaldoPara(7));
    }

    [Fact]
    public void RegistrarEntrada_NaoDeveAlterarAQuantidade_QuandoAOperacaoFalhar()
    {
        var estoque = new Estoque(10);

        Assert.Throws<ArgumentOutOfRangeException>(() => estoque.RegistrarEntrada(0));

        Assert.Equal(10, estoque.Quantidade);
    }
}
