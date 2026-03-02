namespace EmpadronamientoBackend.Application.DTOs.Requests;

/// <summary>
/// Parámetros estándar de paginación con validación de rangos.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Número de página actual (Mínimo 1).
    /// </summary>
    public int PageNumber 
    { 
        get => _pageNumber; 
        set => _pageNumber = value < 1 ? 1 : value; // Previene páginas negativas o 0
    }

    /// <summary>
    /// Cantidad de registros por página (Máximo 100).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }
}