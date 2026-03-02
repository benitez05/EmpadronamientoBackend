using EmpadronamientoBackend.API.Extensions;
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.API.Middleware;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using FluentValidation;
using FluentValidation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region -------------------------------------------------- SERVICES

// Logging base
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Controllers + filtros globales
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

// OpenAPI (Estándar .NET 9)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); 

// FluentValidation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<
    EmpadronamientoBackend.Application.DTOs.Requests.PaginationParams>();

// Infrastructure (MariaDB)
builder.Services.AddInfrastructure(builder.Configuration);

// 🔥 CAMBIO CRÍTICO: Pasamos builder.Configuration para que JWT pueda leer la Key
builder.Services.AddApiTemplate(builder.Configuration);

#endregion

var app = builder.Build();

#region -------------------------------------------------- PIPELINE

// Middleware global de excepciones (SIEMPRE primero)
app.UseMiddleware<ExceptionMiddleware>();

// Logging automático por request
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();

// 🔥 CAMBIO CRÍTICO: Debemos agregar Authentication antes de Authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();