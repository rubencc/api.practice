using Weather.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Configuration;
using Api.Weather.Host.ExceptionHandlers;
using Api.Weather.Host.Configuration;
using Asp.Versioning.ApiExplorer;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services
    .AddEndpointsApiExplorer()
    .AddControllers();

// Configurar API Versioning
builder.Services.AddApiVersioningConfiguration();

// Configurar ProblemDetails y Exception Handler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configurar Swagger con versionado
builder.Services.AddSwaggerConfiguration();

//FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services
    .AddApplicationDependencies()
    .AddInfrastructureDependencies(builder.Configuration);

var app = builder.Build();

// Usar exception handler
app.UseExceptionHandler();

app.UseHttpsRedirection();

// Configurar Swagger UI con múltiples versiones
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
});

app.UseRouting();
app.MapControllers();

app.Run();

