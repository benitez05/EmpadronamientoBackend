using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Responses;

namespace EmpadronamientoBackend.Application.Mappers;

public static class OrganizacionMapper
{
    /// <summary>
    /// Convierte la entidad Organizacion a DTO de respuesta.
    /// Si existe logo y se proporciona el servicio S3, genera una URL temporal.
    /// </summary>
    public static OrganizacionResponse ToResponse(this Organizacion o, IS3Service? s3Service = null)
    {
        if (o == null) return null!;

        Func<string?, string?> generateUrl = fileName =>
        {
            if (string.IsNullOrEmpty(fileName) || s3Service == null)
                return fileName;

            return s3Service.GetPreSignedUrl(fileName);
        };

        return new OrganizacionResponse
        {
            Id = o.Id,
            Nombre = o.Nombre,
            Descripcion = o.Descripcion,
            EmailContacto = o.EmailContacto,
            Telefono = o.Telefono,

            Calle = o.Calle,
            NumeroExterior = o.NumeroExterior,
            NumeroInterior = o.NumeroInterior,
            CP = o.CP,
            Colonia = o.Colonia,
            Municipio = o.Municipio,
            Estado = o.Estado,
            Pais = o.Pais,

            LogoUrl = generateUrl(o.LogoUrl),

            Activa = o.Activa,
            FechaVencimiento = o.FechaVencimiento,

            NombrePlan = o.Plan?.Nombre
        };
    }

    /// <summary>
    /// Convierte una lista de organizaciones a DTOs de respuesta.
    /// </summary>
    public static IEnumerable<OrganizacionResponse> ToResponseList(
        this IEnumerable<Organizacion> orgs,
        IS3Service? s3Service = null)
    {
        return orgs.Select(o => o.ToResponse(s3Service));
    }
}