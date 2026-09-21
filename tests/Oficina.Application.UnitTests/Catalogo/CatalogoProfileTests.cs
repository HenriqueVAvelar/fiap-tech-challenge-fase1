using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Mappings;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Catalogo;

public class CatalogoProfileTests
{
    private static MapperConfiguration CriarConfiguracao()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<CatalogoProfile>();

        return new MapperConfiguration(expression, NullLoggerFactory.Instance);
    }

    private static IMapper CriarMapper() => CriarConfiguracao().CreateMapper();

    [Fact]
    public void Configuracao_DeveSerValida()
    {        
        CriarConfiguracao().AssertConfigurationIsValid();
    }

    [Fact]
    public void DeveMapearTodosOsCampos_IncluindoId()
    {
        var catalogo = new Domain.Entities.Catalogo(
            "Filtro de óleo",
            49.90m,
            EnumsConsumivel.TipoConsumivel.Peca,
            true,
            "Filtro para motores 1.0");

        var dto = CriarMapper().Map<CatalogoDto>(catalogo);

        Assert.Equal(catalogo.Nome, dto.Nome);
        Assert.Equal(catalogo.Preco, dto.Preco);
        Assert.Equal(catalogo.Tipo, dto.Tipo);
        Assert.Equal(catalogo.Ativo, dto.Ativo);
        Assert.Equal(catalogo.Descricao, dto.Descricao);
    }

    [Fact]
    public void DeveMapearColecao()
    {
        var catalogos = new List<Domain.Entities.Catalogo>
        {
            new("Filtro de óleo", 49.90m, EnumsConsumivel.TipoConsumivel.Peca, true),
            new("Óleo 5W30", 89.90m, EnumsConsumivel.TipoConsumivel.Insumo, true)
        };

        var dtos = CriarMapper().Map<IEnumerable<CatalogoDto>>(catalogos).ToList();

        Assert.Equal(2, dtos.Count);
    }
}
