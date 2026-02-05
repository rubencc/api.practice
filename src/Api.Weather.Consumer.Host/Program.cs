using Api.Weather.Consumer.Host.Extensions;
using Weather.Application.Configuration;
using Weather.Infrastructure.Cache.Configuration;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.Opentelemetry.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services
    .AddEndpointsApiExplorer()
    .AddControllers();

// Configurar OpenTelemetry
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

//Redis
builder.Services.AddRedisCaching(builder.Configuration);

builder.Services
    .AddApplicationDependencies()
    .AddInfrastructureDependencies(builder.Configuration);

builder.Services.AddMassTransit(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();