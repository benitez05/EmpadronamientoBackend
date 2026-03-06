namespace BenitezLabs.Domain.Common;

public abstract class AuditoriaEntidad
{
    // Datos de Creación
    public DateTime FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = null!; // Email
    public string? IpCreacion { get; set; }
    public string? DispositivoCreacion { get; set; }

    // Datos de Modificación
    public DateTime? FechaUltimaActualizacion { get; set; }
    public string? ActualizadoPor { get; set; } // Email
    public string? IpUltimaActualizacion { get; set; }
    public string? DispositivoUltimaActualizacion { get; set; }
}