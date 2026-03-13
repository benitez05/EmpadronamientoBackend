using Microsoft.AspNetCore.Http;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class OrganizacionRequest
{
    public string Nombre { get; set; } = default!;

    public string EmailContacto { get; set; } = default!;

    public int PlanId { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string? Descripcion { get; set; }

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

    // Logo opcional
    public IFormFile? Logo { get; set; }

    public bool Activa { get; set; } = true;
}