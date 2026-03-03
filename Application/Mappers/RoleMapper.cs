using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Application.Mappers;

public static class RoleMapper
{
    public static RoleResponse ToResponse(this Role r)
    {
        if (r == null) return null!;

        return new RoleResponse
        {
            Id = r.Id,
            Nombre = r.Nombre,
        };
    }

    public static Role ToEntity(this RoleRequest request)
    {
        return new Role
        {
            Nombre = request.Nombre
        };
    }

    public static IEnumerable<RoleResponse> ToResponseList(this IEnumerable<Role> roles)
    {
        return roles.Select(r => r.ToResponse());
    }
}