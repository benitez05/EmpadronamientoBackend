using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Mappers;

public static class ModuloMapper
{
    // 1. De Entidad a Response (Individual)
    public static ModuloResponse ToResponse(this Modulo m, IS3Service? s3Service = null)
    {
        if (m == null) return null!;

        Func<string?, string?> generateUrl = fileName =>
        {
            if (string.IsNullOrEmpty(fileName) || s3Service == null)
                return fileName;

            // Si es un nombre de icono de fuente (ej: "shield"), no generamos URL de S3
            if (!fileName.Contains(".") && fileName.Length < 30) 
                return fileName;

            return s3Service.GetPreSignedUrl(fileName);
        };

        return new ModuloResponse
        {
            Id = m.Id,
            Nombre = m.Nombre,
            K = m.K,
            Color = m.Color,
            Icono = generateUrl(m.Icono)
        };
    }

    public static Modulo ToEntity(this ModuloRequest request)
{
    if (request == null) return null!;

    return new Modulo
    {
        Nombre = request.Nombre,
        K = request.K,
        Color = string.IsNullOrEmpty(request.Color) ? "#6B7280" : request.Color
        // El Icono NO se mapea aquí porque es un IFormFile en el request
        // y un string en la Entidad. Se setea en el Controller.
    };
}
    // 3. Para mapear colecciones
    public static IEnumerable<ModuloResponse> ToResponseList(
        this IEnumerable<Modulo> modulos, 
        IS3Service? s3Service = null)
    {
        return modulos.Select(m => m.ToResponse(s3Service));
    }
}