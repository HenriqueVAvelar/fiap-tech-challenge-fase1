using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Oficina.Infrastructure.Context;
using Testcontainers.PostgreSql;

namespace Oficina.Api.IntegrationTests.Support;

/// <summary>
/// Sobe a API inteira em memória (pipeline HTTP, JWT, controllers, serviços e EF Core) apontando para um
/// PostgreSQL real em container, com o esquema criado pelas migrations. É compartilhada por todos os testes.
/// </summary>
public class OficinaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUserName = "admin-integracao";
    public const string AdminSenha = "Admin@12345";

    // A mesma versão do compose.yaml
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16.9-bullseye").Build();

    private readonly Dictionary<string, string?> _variaveisAnteriores = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var connectionString = _postgres.GetConnectionString();

        // A aplicação não aplica migrations ao iniciar: o esquema precisa existir antes dos seeds do Program.cs.
        var options = new DbContextOptionsBuilder<OficinaDbContext>().UseNpgsql(connectionString).Options;

        await using (var context = new OficinaDbContext(options))
            await context.Database.MigrateAsync();

        // Variáveis de ambiente valem desde a criação do builder, quando o Program.cs já lê a configuração.
        Definir("ConnectionStrings__DefaultConnection", connectionString);
        Definir("Jwt__Secret", "chave-secreta-dos-testes-de-integracao-0123456789");
        Definir("Jwt__Issuer", "Oficina.Api");
        Definir("Jwt__Audience", "Oficina.Api");
        Definir("AdminSeed__Nome", "Administrador de Integração");
        Definir("AdminSeed__UserName", AdminUserName);
        Definir("AdminSeed__Senha", AdminSenha);
        Definir("DemoData__Habilitado", "false");

        // Menos ruído no console: os logs que interessam são os das requisições HTTP (HttpLogHandler)
        Definir("Logging__LogLevel__Default", "Warning");
        Definir("Logging__LogLevel__Microsoft.EntityFrameworkCore", "Error");
    }

    /// <summary>Cliente HTTP da API que imprime cada requisição e resposta no console.</summary>
    public HttpClient CriarClienteHttp() => CreateDefaultClient(new HttpLogHandler());

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();

        foreach (var (nome, valor) in _variaveisAnteriores)
            Environment.SetEnvironmentVariable(nome, valor);
    }

    private void Definir(string nome, string valor)
    {
        _variaveisAnteriores[nome] = Environment.GetEnvironmentVariable(nome);
        Environment.SetEnvironmentVariable(nome, valor);
    }
}
