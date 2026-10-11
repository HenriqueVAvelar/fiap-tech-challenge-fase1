using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Infrastructure.Security;

public static class AdminSeeder
{
    /// <summary>
    /// Cria o primeiro Administrador a partir da seção "AdminSeed" da configuração.
    /// Não faz nada se a seção estiver incompleta ou se já existir um Administrador.
    /// </summary>
    public static async Task SeedAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminSeeder));

        if (string.IsNullOrWhiteSpace(settings.UserName) || string.IsNullOrWhiteSpace(settings.Senha))
            return;

        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

            var usuarios = await repository.ObterTodosAsync();
            if (usuarios.Any(u => u.Perfil == EnumsUsuario.PerfilUsuario.Administrador))
                return;

            if (await repository.UserNameExisteAsync(settings.UserName))
            {
                logger.LogWarning("Seed do administrador ignorado: o UserName '{UserName}' já está em uso.", settings.UserName);
                return;
            }

            Usuario.ValidarSenha(settings.Senha);

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var nome = string.IsNullOrWhiteSpace(settings.Nome) ? "Administrador" : settings.Nome;

            await repository.AdicionarAsync(new Usuario(
                nome,
                settings.UserName,
                hasher.Hash(settings.Senha),
                EnumsUsuario.PerfilUsuario.Administrador));

            logger.LogInformation("Administrador inicial '{UserName}' criado.", settings.UserName);
        }
        catch (Exception ex)
        {
            // Ex: banco indisponível ou tabela ainda não criada
            logger.LogError(ex, "Não foi possível executar o seed do administrador.");
        }
    }
}
