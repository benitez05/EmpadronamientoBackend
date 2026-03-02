using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Application.Mappers;

public static class ModuloMapper
{
    // De Entidad a Response (Para los GET)
    public static ModuloResponse ToResponse(this Modulo m)
    {
        return new ModuloResponse
        {
            Id = m.Id,
            Nombre = m.Nombre,
            K = m.K
        };
    }

    // De Request a Entidad (Para los POST/Create)
    public static Modulo ToEntity(this ModuloRequest request)
    {
        return new Modulo
        {
            Nombre = request.Nombre,
            K = request.K
        };
    }

    // Para mapear listas rápido
    public static IEnumerable<ModuloResponse> ToResponseList(this IEnumerable<Modulo> modulos)
    {
        return modulos.Select(m => m.ToResponse());
    }
}