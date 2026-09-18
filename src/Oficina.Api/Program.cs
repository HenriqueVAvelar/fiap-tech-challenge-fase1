using Oficina.Application.Extensions;
using Oficina.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Injeção de Dependências das Camadas
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configurações Globais da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();