using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;
using System.Collections.Generic;
using System.Linq;

namespace EmpadronamientoBackend.Application.Mappers;

public static class ModuloMapper
{
    // 1. De Entidad a Response (Individual)
    public static ModuloResponse ToResponse(this Modulo m, IS3Service? s3Service = null)
    {
        if (m == null) return null!;

        string? ProcessIcono(string? iconoValue)
        {
            if (string.IsNullOrEmpty(iconoValue) || s3Service == null)
                return iconoValue;

            if (!iconoValue.Contains(".") && iconoValue.Length < 30) 
                return iconoValue;

            return s3Service.GetFileUrl(iconoValue);
        }

        return new ModuloResponse
        {
            Id = m.Id,
            Nombre = m.Nombre,
            K = m.K,
            Color = m.Color ?? "#6B7280",
            Icono = ProcessIcono(m.Icono),
            // 🔥 AGREGADO: Mapeo al Response
            Multinivel = m.Multinivel 
        };
    }

    // 2. De Request a Entidad
    public static Modulo ToEntity(this ModuloRequest request)
    {
        if (request == null) return null!;

        return new Modulo
        {
            Nombre = request.Nombre,
            K = request.K,
            Color = string.IsNullOrEmpty(request.Color) ? "#6B7280" : request.Color,
            // 🔥 AGREGADO: Mapeo del Request a la Entidad
            // Si en el DTO es bool?, usa .GetValueOrDefault(true) para respetar tu default
            Multinivel = request.Multinivel ?? true 
        };
    }

    public static IEnumerable<ModuloResponse> ToResponseList(
        this IEnumerable<Modulo> modulos, 
        IS3Service? s3Service = null)
    {
        if (modulos == null) return Enumerable.Empty<ModuloResponse>();
        return modulos.Select(m => m.ToResponse(s3Service));
    }
}