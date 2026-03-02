using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Application.Mappers;

public static class RolePermisoMapper
{
    public static PermisoMatrixResponse ToMatrixResponse(this RolePermiso rp)
    {
        if (rp == null) return null!;

        return new PermisoMatrixResponse
        {
            Rol = rp.Role?.Nombre ?? "N/A",
            Modulo = rp.Modulo?.Nombre ?? "N/A",
            Key = rp.Modulo?.K ?? "N/A",
            Nivel = rp.Lvl,
            DescripcionNivel = rp.Lvl switch
            {
                1 => "Lectura",
                2 => "Escritura",
                3 => "Eliminación",
                4 or 5 => "Especial",
                _ => "Desconocido"
            }
        };
    }

    public static IEnumerable<PermisoMatrixResponse> ToMatrixList(this IEnumerable<RolePermiso> permisos)
    {
        return permisos.Select(p => p.ToMatrixResponse());
    }
}