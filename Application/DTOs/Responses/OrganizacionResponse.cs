namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class OrganizacionResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = default!;

    public string? Descripcion { get; set; }

    public string? EmailContacto { get; set; }

    public string? Telefono { get; set; }

    // Dirección estructurada
    public string Calle { get; set; } = string.Empty;

    public string NumeroExterior { get; set; } = string.Empty;

    public string NumeroInterior { get; set; } = string.Empty;

    public int CP { get; set; }

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Pais { get; set; }

    // Imagen
    public string? LogoUrl { get; set; }

    // Estado
    public bool Activa { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string? NombrePlan { get; set; }
}