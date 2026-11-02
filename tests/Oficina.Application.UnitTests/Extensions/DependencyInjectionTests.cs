using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Extensions;
using Oficina.Application.Interfaces;
using Oficina.Application.Services;

namespace Oficina.Application.UnitTests.Extensions;

public class DependencyInjectionTests
{
    public static TheoryData<Type, Type> Servicos => new()
    {
        { typeof(ICatalogoService), typeof(CatalogoService) },
        { typeof(IClienteService), typeof(ClienteService) },
        { typeof(IEstoqueService), typeof(EstoqueService) },
        { typeof(IOrdemServicoService), typeof(OrdemServicoService) },
        { typeof(IServicoService), typeof(ServicoService) },
        { typeof(ITempoMedioExecucaoService), typeof(TempoMedioExecucaoService) },
        { typeof(IVeiculoService), typeof(VeiculoService) },
        { typeof(IUsuarioService), typeof(UsuarioService) },
        { typeof(IAuthService), typeof(AuthService) }
    };

    [Theory]
    [MemberData(nameof(Servicos))]
    public void AddApplication_DeveRegistrarOServicoComoScoped(Type contrato, Type implementacao)
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descritor = Assert.Single(services, d => d.ServiceType == contrato);
        Assert.Equal(implementacao, descritor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descritor.Lifetime);
    }

    [Fact]
    public void AddApplication_DeveRetornarAMesmaColecao_ParaEncadeamento()
    {
        var services = new ServiceCollection();

        var retorno = services.AddApplication();

        Assert.Same(services, retorno);
    }
}
