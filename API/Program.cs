using EmpadronamientoBackend.API.Extensions;
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.API.Middleware;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models; // Necesario para OpenApiServer
using FluentValidation;
using FluentValidation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region -------------------------------------------------- SERVICES

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 1. Configurar CORS para que tu PC pueda hablarle al Server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalScalar", policy =>
    {
        policy.AllowAnyOrigin() // O pon tu IP de casa para más seguridad
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();

// 2. Configurar el Selector de URLs en OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "http://localhost:5000", Description = "Local (Desarrollo)" },
            new OpenApiServer { Url = "http://143.198.231.51:5000", Description = "DigitalOcean (Producción)" }
        };
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

// 3. Habilitar CORS antes de los mapas
app.UseCors("AllowLocalScalar");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();