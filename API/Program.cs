using EmpadronamientoBackend.API.Extensions;
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.API.Middleware;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt; // <--- AGREGAR ESTO

// 🔥 PASO 1: LIMPIEZA ABSOLUTA DE MAPEOS DE MICROSOFT
// Esto va antes de cualquier otra cosa para que no te salgan los links largos
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

#region -------------------------------------------------- SERVICES

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalScalar", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Ya no necesitas AddEndpointsApiExplorer con .NET 9 y AddOpenApi, pero lo dejamos si gustas.

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Personalizamos el título que verás en Scalar
        document.Info.Title = "BenitezLabs - Sistema de Empadronamiento";
        document.Info.Description = "API de alto rendimiento para gestión de usuarios y permisos.";
        
        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "http://143.198.231.51:5000", Description = "DigitalOcean (Producción)" },
            new OpenApiServer { Url = "http://localhost:5034", Description = "Local (Desarrollo)" }
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        return Task.CompletedTask;
    });
});

builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<EmpadronamientoBackend.Application.DTOs.Requests.PaginationParams>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiTemplate(builder.Configuration);

#endregion

var app = builder.Build();

#region -------------------------------------------------- PIPELINE

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowLocalScalar");

// 🔥 PASO 2: MAPEO DE OPENAPI (Sácalo del IF para que el dev vea el JSON en el server)
app.MapOpenApi(); 

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("Empadronamiento Docs")
               .WithTheme(ScalarTheme.Moon);
    }); 
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();