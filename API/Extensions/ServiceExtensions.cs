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
using EmpadronamientoBackend.API.Filters;
using EmpadronamientoBackend.Infrastructure.Identity;
using EmpadronamientoBackend.Application.DTOs.Responses;

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
                document.Info.Title = "BenitezLabs Enterprise API";
                document.Info.Version = "v1";
                document.Info.Description = "Template de alto rendimiento con Clean Architecture.";

                // DEFINICIÓN DEL ESQUEMA DE SEGURIDAD
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

                // APLICAR REQUERIMIENTO GLOBAL (Para que Scalar lo inyecte solo)
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
                    // Buscamos el atributo [DefaultValue] en la clase C#
                    var propInfo = type.GetProperty(property.Key, 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    
                    var defaultAttr = propInfo?.GetCustomAttribute<DefaultValueAttribute>();

                    if (defaultAttr != null)
                    {
                        // Inyectamos el valor al ejemplo que lee Scalar
                        property.Value.Example = new OpenApiString(defaultAttr.Value?.ToString());
                    }
                }
                return Task.CompletedTask;
            });
        });

        // 4. COMPORTAMIENTO DE API PERSONALIZADO
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // 5. SERVICIOS DE IDENTIDAD Y SEGURIDAD
        services.AddScoped<PasswordService>();

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };

                // 🔥 METE ESTO AQUÍ ABAJO:
                options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // Usamos tu Factory con tipo 'object' porque no hay Data
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