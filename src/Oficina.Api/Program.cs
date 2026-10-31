using dotenv.net;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Oficina.Api.Extensions;
using Oficina.Application.Extensions;
using Oficina.Infrastructure.Extensions;
using Oficina.Infrastructure.Security;
using Oficina.Infrastructure.Seed;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Injeção de Dependências das Camadas
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configurações de Observabilidade (OpenTelemetry)
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());

// Configurações Globais da API
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe o token JWT obtido em POST /Auth/login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Expor endpoint de métricas para o Prometheus
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Aplica migrations automaticamente
await app.Services.ApplyMigrationsAsync();

// Cria o primeiro Administrador (seção "AdminSeed" da configuração), se ainda não existir
await app.Services.SeedAdminAsync();

// Cria as peças e insumos padrão do catálogo e, se habilitado (seção "DemoData"), cliente e veículo de demonstração
await app.Services.SeedDadosDemoAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();