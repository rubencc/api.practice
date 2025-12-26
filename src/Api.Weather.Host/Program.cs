using Weather.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Configuration;
using Api.Weather.Host.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services
    .AddEndpointsApiExplorer()
    .AddControllers();

// Configurar ProblemDetails y Exception Handler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddSwaggerGen()
    .AddApplicationDependencies()
    .AddInfrastructureDependencies(builder.Configuration);

var app = builder.Build();

// Usar exception handler
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.Run();

