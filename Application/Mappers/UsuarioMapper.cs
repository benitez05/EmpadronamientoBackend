using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace EmpadronamientoBackend.Application.Mappers;

public static class UsuarioMapper
{
    /// <summary>
    /// Convierte una entidad Usuario a un DTO de respuesta.
    /// Genera la URL pública directa (sin firma) para mejor rendimiento.
    /// </summary>
    public static UsuarioResponse ToResponse(this Usuario u, IS3Service? s3Service = null)
    {
        if (u == null) return null!;

        // Función interna que genera URL pública
        string? GenerateUrl(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || s3Service == null)
                return fileName; 

            // 🔥 Usamos GetFileUrl para que la URL sea estática y cacheable
            return s3Service.GetFileUrl(fileName);
        };

        return new UsuarioResponse
        {
            Id = u.Id,
            Correo = u.Correo,
            Nombre = u.Nombre,
            Apellidos = u.Apellidos,
            Celular = u.Celular,
            Tipo = u.Tipo,
            // Mapeo del Rol (Asegúrate de tener el ToResponse en el mapper de Roles)
            Rol = u.Role?.ToResponse(), 
            ImagenUrl = GenerateUrl(u.Imagen),
            CorreoConfirmado = u.CorreoConfirmado
        };
    }

    /// <summary>
    /// Convierte un Request de Registro a una Entidad de Dominio.
    /// </summary>
    public static Usuario ToEntity(this RegisterRequest request)
    {
        if (request == null) return null!;

        return new Usuario
        {
            Nombre = request.Nombre,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            PasswordHash = string.Empty,
            RoleId = 2, // Por defecto nivel Usuario
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convierte una lista de entidades a una lista de respuestas.
    /// </summary>
    public static IEnumerable<UsuarioResponse> ToResponseList(this IEnumerable<Usuario> usuarios, IS3Service? s3Service = null)
    {
        if (usuarios == null) return Enumerable.Empty<UsuarioResponse>();
        
        // Pasamos el s3Service para generar las URLs públicas de toda la lista
        return usuarios.Select(u => u.ToResponse(s3Service));
    }
}