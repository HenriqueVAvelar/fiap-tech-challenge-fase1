using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs;
using Oficina.Application.Mappings;
using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.UnitTests.Cadastro;

public class CadastroProfilesTests
{
    private static MapperConfiguration CriarConfiguracao()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<ClienteProfile>();
        expression.AddProfile<VeiculoProfile>();
        expression.AddProfile<ServicoProfile>();
        expression.AddProfile<EstoqueProfile>();

        return new MapperConfiguration(expression, NullLoggerFactory.Instance);
    }

    [Fact]
    public void Configuracao_DeveSerValida()
    {
        CriarConfiguracao().AssertConfigurationIsValid();
    }

    [Fact]
    public void Cliente_DeveMapearDocumentoSomenteComDigitos_ETipo()
    {
        var cliente = new Cliente("Maria", new Documento("529.982.247-25"));

        var dto = CriarConfiguracao().CreateMapper().Map<ClienteDto>(cliente);

        Assert.Equal("52998224725", dto.Documento);
        Assert.Equal(Domain.Enums.EnumsDocumento.TipoDocumento.Cpf, dto.TipoDocumento);
    }

    [Fact]
    public void Veiculo_DeveMapearPlacaComoTexto()
    {
        var veiculo = new Veiculo(1, new Placa("abc-1d23"), "Fiat", "Uno", 2020);

        var dto = CriarConfiguracao().CreateMapper().Map<VeiculoDto>(veiculo);

        Assert.Equal("ABC1D23", dto.Placa);
    }
}
