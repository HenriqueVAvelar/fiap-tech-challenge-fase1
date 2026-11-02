using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oficina.Application.Interfaces;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;
using Oficina.Infrastructure.Extensions;
using Oficina.Infrastructure.Repositories;
using Oficina.Infrastructure.Security;
using Oficina.Infrastructure.Seed;

namespace Oficina.Infrastructure.UnitTests.Extensions;

public class DependencyInjectionTests
{
    private static IConfiguration CriarConfiguracao() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=di_teste",
            ["Jwt:Secret"] = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres",
            ["Jwt:Issuer"] = "Oficina.Api",
            ["Jwt:Audience"] = "Oficina.Web",
            ["Jwt:ExpirationMinutes"] = "15",
            ["AdminSeed:Nome"] = "Chefe",
            ["AdminSeed:UserName"] = "admin",
            ["AdminSeed:Senha"] = "Senha@123",
            ["DemoData:Habilitado"] = "true"
        })
        .Build();

    public static TheoryData<Type, Type> Repositorios => new()
    {
        { typeof(ICatalogoRepository), typeof(CatalogoRepository) },
        { typeof(IClienteRepository), typeof(ClienteRepository) },
        { typeof(IEstoqueRepository), typeof(EstoqueRepository) },
        { typeof(IOrdemServicoRepository), typeof(OrdemServicoRepository) },
        { typeof(IServicoRepository), typeof(ServicoRepository) },
        { typeof(IVeiculoRepository), typeof(VeiculoRepository) },
        { typeof(IUsuarioRepository), typeof(UsuarioRepository) }
    };

    [Theory]
    [MemberData(nameof(Repositorios))]
    public void AddInfrastructure_DeveRegistrarORepositorioComoScoped(Type contrato, Type implementacao)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(CriarConfiguracao());

        var descritor = Assert.Single(services, d => d.ServiceType == contrato);
        Assert.Equal(implementacao, descritor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descritor.Lifetime);
    }

    [Theory]
    [InlineData(typeof(IPasswordHasher), typeof(PasswordHasher))]
    [InlineData(typeof(ITokenService), typeof(JwtTokenService))]
    public void AddInfrastructure_DeveRegistrarSegurancaComoSingleton(Type contrato, Type implementacao)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(CriarConfiguracao());

        var descritor = Assert.Single(services, d => d.ServiceType == contrato);
        Assert.Equal(implementacao, descritor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descritor.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_DeveRegistrarODbContext()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(CriarConfiguracao());

        Assert.Contains(services, d => d.ServiceType == typeof(OficinaDbContext));
    }

    [Fact]
    public void AddInfrastructure_DeveRetornarAMesmaColecao_ParaEncadeamento()
    {
        var services = new ServiceCollection();

        Assert.Same(services, services.AddInfrastructure(CriarConfiguracao()));
    }

    [Fact]
    public void AddInfrastructure_DeveVincularAsConfiguracoesAsSecoes()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CriarConfiguracao());
        using var provider = services.BuildServiceProvider();

        var jwt = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
        var admin = provider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
        var demo = provider.GetRequiredService<IOptions<DemoDataSettings>>().Value;

        Assert.Equal("chave-de-teste-com-mais-de-trinta-e-dois-caracteres", jwt.Secret);
        Assert.Equal("Oficina.Api", jwt.Issuer);
        Assert.Equal("Oficina.Web", jwt.Audience);
        Assert.Equal(15, jwt.ExpirationMinutes);
        Assert.Equal("Chefe", admin.Nome);
        Assert.Equal("admin", admin.UserName);
        Assert.Equal("Senha@123", admin.Senha);
        Assert.True(demo.Habilitado);
    }

    [Fact]
    public void AddInfrastructure_DeveManterOsPadroes_QuandoAsSecoesNaoExistem()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();

        var jwt = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
        var demo = provider.GetRequiredService<IOptions<DemoDataSettings>>().Value;

        Assert.Equal(60, jwt.ExpirationMinutes);
        Assert.False(demo.Habilitado);
    }

    [Fact]
    public void AddInfrastructure_DevePermitirResolverSegurancaERepositorios()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CriarConfiguracao());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<PasswordHasher>(provider.GetRequiredService<IPasswordHasher>());
        Assert.IsType<JwtTokenService>(provider.GetRequiredService<ITokenService>());
        Assert.IsType<ClienteRepository>(scope.ServiceProvider.GetRequiredService<IClienteRepository>());
        Assert.IsType<UsuarioRepository>(scope.ServiceProvider.GetRequiredService<IUsuarioRepository>());
    }

    [Fact]
    public void AddInfrastructure_DeveReutilizarAMesmaInstancia_DeSeguranca()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CriarConfiguracao());
        using var provider = services.BuildServiceProvider();

        Assert.Same(
            provider.GetRequiredService<IPasswordHasher>(),
            provider.GetRequiredService<IPasswordHasher>());
    }
}
