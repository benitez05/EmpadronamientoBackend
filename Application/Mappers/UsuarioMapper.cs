using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.Interfaces;

namespace EmpadronamientoBackend.Application.Mappers;

public static class UsuarioMapper
{
    /// <summary>
/// Convierte una entidad Usuario a un DTO de respuesta.
/// Genera automáticamente la URL temporal si hay imagen.
/// </summary>
public static UsuarioResponse ToResponse(this Usuario u, IS3Service? s3Service = null)
{
    if (u == null) return null!;

    // Función interna que genera URL temporal si se proporciona el servicio
    Func<string?, string?> generateUrl = fileName =>
    {
        if (string.IsNullOrEmpty(fileName) || s3Service == null)
            return fileName; // devuelve null o el nombre si no hay servicio
        return s3Service.GetPreSignedUrl(fileName);
    };

    return new UsuarioResponse
    {
        Id = u.Id,
        Correo = u.Correo,
        Nombre = u.Nombre,
        Apellidos = u.Apellidos,
        Celular = u.Celular,
        Tipo = u.Tipo,
        Rol = u.Role?.ToResponse(),
        ImagenUrl = generateUrl(u.Imagen) // automáticamente genera la URL si hay imagen
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