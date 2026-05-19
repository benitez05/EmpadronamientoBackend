using EmpadronamientoBackend.API.Extensions;
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.API.Middleware;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore; // 🔥 IMPORTANTE PARA dbContext.Database.Migrate()

// 🔥 USINGS PARA AWS
using Amazon.Rekognition;
using Amazon.Rekognition.Model;

// 1. Limpieza de mapeos de JWT
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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "BenitezLabs - Sistema de Empadronamiento";
        document.Info.Description = "API de alto rendimiento para gestión de usuarios y permisos.";
        
        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "http://143.198.231.51:5000", Description = "DigitalOcean (Producción)" },
            new OpenApiServer { Url = "http://localhost:5034", Description = "Local (Desarrollo)" },
            new OpenApiServer { Url = "https://emp-spii.robertobenitezg.com", Description = "(Desarrollo servidor )" }
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

// =================================================================
// 🔥 AUTO-VERIFICADOR DE REKOGNITION (Solo Empadronamiento) 🔥
// =================================================================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try 
    {
        var rekognitionClient = scope.ServiceProvider.GetRequiredService<IAmazonRekognition>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var collectionId = config["AWS:Rekognition:Collections:Empadronamiento"];

        if (!string.IsNullOrEmpty(collectionId))
        {
            try
            {
                await rekognitionClient.DescribeCollectionAsync(new DescribeCollectionRequest 
                { 
                    CollectionId = collectionId 
                });
                logger.LogInformation("[AWS] Colección de Empadronamiento '{CollectionId}' verificada y activa.", collectionId);
            }
            catch (ResourceNotFoundException)
            {
                logger.LogWarning("[AWS] La colección '{CollectionId}' no existe. Creándola ahora...", collectionId);
                await rekognitionClient.CreateCollectionAsync(new CreateCollectionRequest 
                { 
                    CollectionId = collectionId 
                });
                logger.LogInformation("[AWS] ¡Colección '{CollectionId}' creada exitosamente!", collectionId);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[AWS] Error crítico al inicializar Rekognition.");
    }
}
// =================================================================

// =================================================================
// 🔥 AUTO-MIGRACIÓN DE BASE DE DATOS 🔥
// =================================================================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try 
    {
        // ⚠️ REEMPLAZA 'ApplicationDbContext' POR EL NOMBRE REAL DE TU CONTEXTO
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Esto crea la BD si no existe y aplica las migraciones pendientes
        dbContext.Database.Migrate(); 
        logger.LogInformation("[BD] Base de datos verificada y migrada correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[BD] Error crítico al inicializar o migrar la base de datos.");
    }
}
// =================================================================

#region -------------------------------------------------- PIPELINE

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowLocalScalar");

app.MapOpenApi(); 

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("Empadronamiento Docs")
               .WithTheme(ScalarTheme.Moon);
    }); 
}

//app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();