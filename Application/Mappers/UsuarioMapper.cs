using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Application.Mappers;

public static class UsuarioMapper
{
    /// <summary>
    /// Convierte una entidad Usuario a un DTO de respuesta.
    /// Incluye el mapeo del Rol asociado.
    /// </summary>
    public static UsuarioResponse ToResponse(this Usuario u)
    {
        if (u == null) return null!;

        return new UsuarioResponse
        {
            Id = u.Id,
            Correo = u.Correo,
            Nombre = u.Nombre,
            Apellidos = u.Apellidos,
            Celular = u.Celular,
            Rol = u.Role?.ToResponse() 
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
    public static IEnumerable<UsuarioResponse> ToResponseList(this IEnumerable<Usuario> usuarios)
    {
        return usuarios.Select(u => u.ToResponse());
    }
}