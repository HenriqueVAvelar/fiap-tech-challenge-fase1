using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.UnitTests.Entities;

public class VeiculoTests
{
    [Fact]
    public void Construtor_DeveAtribuirOsDados()
    {
        var veiculo = new Veiculo(7, new Placa("abc-1234"), "Fiat", "Uno", 2020);

        Assert.Equal(7, veiculo.ClienteId);
        Assert.Equal("ABC1234", veiculo.Placa.Valor);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Uno", veiculo.Modelo);
        Assert.Equal(2020, veiculo.Ano);
        Assert.Null(veiculo.Cliente);
    }
}
