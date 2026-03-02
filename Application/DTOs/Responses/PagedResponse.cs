namespace EmpadronamientoBackend.Application.DTOs.Responses;

/// <summary>
/// Respuesta estándar paginada.
/// </summary>
public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public PagedResponse(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message)
    {
        Success = true;
        Message = message;
        Data = data;

        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;

        TotalPages = (int)Math.Ceiling(
            totalRecords / (double)pageSize);
    }
}