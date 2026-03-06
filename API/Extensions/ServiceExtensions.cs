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
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.Infrastructure.Identity;
using EmpadronamientoBackend.Infrastructure.Services; // Nuevo para CurrentUserService
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
        // 1. CONFIGURACIÓN DE CONTROLADORES Y FILTROS
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        // 2. CONFIGURACIÓN DE FLUENTVALIDATION
        services.AddValidatorsFromAssembly(typeof(EmpadronamientoBackend.Application.DTOs.Requests.LoginRequest).Assembly);

        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });

        // 3. CONFIGURACIÓN DE OPENAPI (.NET 9 + SCALAR)
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            // --- TRANSFORMADOR DE DOCUMENTO (SEGURIDAD GLOBAL) ---
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
                    Description = "Introduce tu token JWT. Scalar lo usará en todos los endpoints con [Authorize]."
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

            // --- TRANSFORMADOR DE ESQUEMA (EJEMPLOS [DefaultValue]) ---
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

        // 4. COMPORTAMIENTO DE API PERSONALIZADO
        // 4. COMPORTAMIENTO DE API PERSONALIZADO
        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Deshabilitamos el filtro automático para que nuestro ValidationFilter 
            // o esta fábrica manejen la respuesta.
            options.SuppressModelStateInvalidFilter = false;

            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e =>
                    {
                        var error = e.Value!.Errors.First();
                        var mensaje = error.ErrorMessage;

                        // --- MEJORA: Detectar errores de conversión de JSON (como fechas) ---
                        if (string.IsNullOrEmpty(mensaje) && error.Exception != null)
                        {
                            // Si el error es por formato de fecha en fechaVencimiento
                            if (e.Key.Contains("fechaVencimiento", StringComparison.OrdinalIgnoreCase))
                            {
                                return "El formato de 'fechaVencimiento' es inválido. Use ISO 8601 (Ej: 2026-03-06T00:00:00Z).";
                            }

                            // Error genérico de conversión
                            return $"Error de formato en el campo '{e.Key}': el valor enviado no es válido.";
                        }

                        return mensaje;
                    }).ToList();

                // Usamos tu ApiResponseFactory para mantener la estética de BenitezLabs
                var response = ApiResponseFactory.Fail<object>(
                    "Error de validación en la estructura de los datos.",
                    errors: errors,
                    code: "VALIDATION_ERROR"
                );

                return new BadRequestObjectResult(response);
            };
        });

        // 5. SERVICIOS DE IDENTIDAD, CACHE Y CONTEXTO (LO NUEVO)
        services.AddScoped<IPasswordService, PasswordService>();

        // Habilita el acceso al HttpContext (Necesario para CurrentUserService)
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.Sub
            };

            options.Events = new JwtBearerEvents
            {
                // Validación de sesión activa (Logout real)
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

                    var modulo = context.HttpContext.Items["AuthError_Key"]?.ToString() ?? "desconocido";
                    var nivelRequerido = context.HttpContext.Items["AuthError_Level"]?.ToString() ?? "0";

                    string desc = nivelRequerido switch
                    {
                        "1" => "Lectura",
                        "2" => "Escritura",
                        "3" => "Eliminación",
                        "4" or "5" => "Permiso Especial / Auditoría",
                        _ => "Acceso Restringido"
                    };

                    var mensaje = $"Acceso denegado. Requieres nivel {nivelRequerido} ({desc}) en el módulo '{modulo}'.";
                    var response = ApiResponseFactory.Fail<object>(mensaje, code: "AUTH_403");

                    await context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}