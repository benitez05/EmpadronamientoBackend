using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Extensions;

/// <summary>
/// Extensiones IQueryable para paginación independiente del ORM.
/// Compatible con cualquier proveedor de datos.
/// </summary>
public static class QueryableExtensions
{
    public static (List<T> Data, int TotalRecords)
        ToPaged<T>(
            this IQueryable<T> query,
            PaginationParams pagination)
    {
        var total = query.Count();

        var data = query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return (data, total);
    }
}