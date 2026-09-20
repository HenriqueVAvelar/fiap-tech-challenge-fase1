using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Interfaces;
using Oficina.Application.Services;

namespace Oficina.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Services
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        return services;
    }

}
