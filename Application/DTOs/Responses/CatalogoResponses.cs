namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class CatalogoResponse
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<CatalogoItemResponse> Items { get; set; } = new();
}

public class CatalogoItemResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
}