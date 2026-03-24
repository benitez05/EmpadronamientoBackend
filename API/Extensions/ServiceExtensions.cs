using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;

using Amazon.S3;
using Amazon.Rekognition;
using Amazon.Extensions.NETCore.Setup;
using Resend; // <-- NUEVO: Agregado para poder usar ResendClientOptions y AddResend

using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.Infrastructure.Identity;
using EmpadronamientoBackend.Infrastructure.Services;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Infrastructure.Cache;
using EmpadronamientoBackend.Application.Interfaces;

namespace EmpadronamientoBackend.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiTemplate(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        // 1. CONTROLADORES Y FILTROS
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });


        // 2. FLUENTVALIDATION
        services.AddValidatorsFromAssembly(
            typeof(EmpadronamientoBackend.Application.DTOs.Requests.LoginRequest).Assembly
        );

        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });


        // 3. OPENAPI
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Empadronamiento API";
                document.Info.Version = "v1";
                document.Info.Description = "entorno de pruebas sistema de empadronamiento";

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Introduce tu token JWT."
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes.Add("Bearer", securityScheme);

                var requirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };

                document.SecurityRequirements.Add(requirement);

                return Task.CompletedTask;
            });

            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type == null || schema.Properties == null)
                    return Task.CompletedTask;

                var type = context.JsonTypeInfo.Type;

                foreach (var property in schema.Properties)
                {
                    var propInfo = type.GetProperty(property.Key,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    var defaultAttr = propInfo?.GetCustomAttribute<DefaultValueAttribute>();

                    if (defaultAttr != null)
                    {
                        property.Value.Example = new OpenApiString(defaultAttr.Value?.ToString());
                    }
                }

                return Task.CompletedTask;
            });
        });


        // 4. RESPUESTA PERSONALIZADA DE VALIDACIÓN
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;

            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e =>
                    {
                        var error = e.Value!.Errors.First();
                        var mensaje = error.ErrorMessage;

                        if (string.IsNullOrEmpty(mensaje) && error.Exception != null)
                        {
                            if (e.Key.Contains("fechaVencimiento", StringComparison.OrdinalIgnoreCase))
                            {
                                return "El formato de 'fechaVencimiento' es inválido. Use ISO 8601.";
                            }

                            return $"Error de formato en el campo '{e.Key}'.";
                        }

                        return mensaje;
                    }).ToList();

                var response = ApiResponseFactory.Fail<object>(
                    "Error de validación en la estructura de los datos.",
                    errors: errors,
                    code: "VALIDATION_ERROR"
                );

                return new BadRequestObjectResult(response);
            };
        });


        // 5. SERVICIOS DE IDENTIDAD Y CACHE
        services.AddScoped<IPasswordService, PasswordService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();


        // 6. AUTENTICACIÓN JWT
        var jwtKey = configuration["Jwt:Key"] ?? "ClaveTemporalDeSeguridadBenitezLabs2026";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.Sub
            };

            options.Events = new JwtBearerEvents
            {

                OnTokenValidated = async context =>
                {
                    var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                    if (string.IsNullOrEmpty(jti) || await cache.GetAsync<bool?>($"revoked_{jti}") == true)
                    {
                        context.Fail("Este token ha sido revocado.");
                    }
                },

                OnChallenge = async context =>
                {
                    context.HandleResponse();

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponseFactory.Fail<object>(
                        "No estás autorizado. El token es inválido o expiró.",
                        code: "AUTH_401"
                    );

                    await context.Response.WriteAsJsonAsync(response);
                },

                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponseFactory.Fail<object>(
                        "No tienes permisos para acceder a este recurso.",
                        code: "AUTH_403"
                    );

                    await context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        services.AddAuthorization();


        // 7. CONFIGURACIÓN AWS (S3 + REKOGNITION)

        var awsOptions = configuration.GetAWSOptions();

        services.AddDefaultAWSOptions(awsOptions);

        // Cliente S3
        services.AddAWSService<IAmazonS3>();

        // Cliente Rekognition
        services.AddAWSService<IAmazonRekognition>();

        // Servicios de infraestructura
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<IRekognitionService, RekognitionService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();


        // 8. CONFIGURACIÓN RESEND (CORREOS) 

        // 8.1 Cargamos la configuración del appsettings.json
        services.Configure<ResendClientOptions>(configuration.GetSection("Resend"));

        // 8.2 Registramos el cliente de Resend de forma manual (esto reemplaza el AddResend mágico)
        services.AddHttpClient<IResend, ResendClient>();

        // 8.3 Registramos nuestra propia interfaz e implementación de correos
        services.AddScoped<IEmailService, ResendEmailService>();


        return services;
    }
}