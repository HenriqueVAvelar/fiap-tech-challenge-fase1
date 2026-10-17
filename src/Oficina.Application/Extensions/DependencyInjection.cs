using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Interfaces;
using Oficina.Application.Services;

namespace Oficina.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        //Services
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

}
