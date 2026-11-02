using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oficina.Infrastructure.Context;
using Oficina.Infrastructure.Extensions;
using Oficina.Infrastructure.UnitTests.Support;

namespace Oficina.Infrastructure.UnitTests.Extensions;

public class ApplyMigrationsTests
{
    [Fact]
    public async Task ApplyMigrations_DeveRegistrarOErroERelancar_QuandoODbContextNaoEstaRegistrado()
    {
        var logs = new LoggerFactoryCapture();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(logs);
        using var provider = services.BuildServiceProvider();

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.ApplyMigrationsAsync());

        var erro = Assert.Single(logs.Entradas, e => e.Nivel == LogLevel.Error);
        Assert.Contains("migrating the database", erro.Mensagem);
        Assert.IsType<InvalidOperationException>(erro.Excecao);
    }

    [Fact]
    public async Task ApplyMigrations_DeveRegistrarOErroERelancar_QuandoOBancoEstaInacessivel()
    {
        var logs = new LoggerFactoryCapture();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(logs);
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseNpgsql("Host=127.0.0.1;Port=1;Database=inexistente;Username=x;Password=x;Timeout=2;Command Timeout=2"));
        using var provider = services.BuildServiceProvider();

        await Assert.ThrowsAnyAsync<Exception>(() => provider.ApplyMigrationsAsync());

        Assert.Contains(logs.Entradas, e => e.Nivel == LogLevel.Error && e.Excecao is not null);
    }
}
