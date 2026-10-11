using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Interfaces;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;
using Oficina.Infrastructure.Repositories;
using Oficina.Infrastructure.Security;
using Oficina.Infrastructure.Seed;

namespace Oficina.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEstoqueRepository, EstoqueRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Security
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AdminSeedSettings>(configuration.GetSection(AdminSeedSettings.SectionName));
        services.Configure<DemoDataSettings>(configuration.GetSection(DemoDataSettings.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}