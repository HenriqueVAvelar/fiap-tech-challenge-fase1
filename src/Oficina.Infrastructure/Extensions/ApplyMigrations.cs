using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        try
        {
            var context = provider.GetRequiredService<OficinaDbContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseMigrationExtensions));
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}
