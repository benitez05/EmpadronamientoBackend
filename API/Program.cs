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
            new OpenApiServer { Url = "http://localhost:5034", Description = "Local (Desarrollo)" }
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
        
        // Lee la colección (Empadronamiento-Dev o Empadronamiento-Prod)
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
        // Si AWS no responde, logueamos el error pero permitimos que la API inicie
        logger.LogError(ex, "[AWS] Error crítico al inicializar Rekognition.");
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

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();