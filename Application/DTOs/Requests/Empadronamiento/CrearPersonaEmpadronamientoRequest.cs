namespace EmpadronamientoBackend.Application.DTOs.Requests;
public class CrearPersonaEmpadronamientoRequest
{
    // ============================
    // DATOS GENERALES
    // ============================

    public string Nombre { get; set; } = string.Empty;

    public string ApellidoPaterno { get; set; } = string.Empty;

    public string ApellidoMaterno { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }

    public int? Edad { get; set; }

    public decimal? Estatura { get; set; }

    public string? Sexo { get; set; }

    public string? Originario { get; set; }

    public string? Telefono { get; set; }

    public string? Apodo { get; set; }

    public string? Nacionalidad { get; set; }

    public required string EstadoCivil { get; set; }

    public required string Escolaridad { get; set; }

    public required string OficioProfesion { get; set; }

    public string? ObservacionesGenerales { get; set; }

    /// <summary>
    /// Observaciones dentro del empadronamiento
    /// (tabla intermedia)
    /// </summary>
    public string? ObservacionesEmpadronamiento { get; set; }

    // ============================
    // RELACIONES OBLIGATORIAS
    // ============================

    public CrearRostroRequest Rostro { get; set; } = null!;

    public CrearDireccionRequest Direccion { get; set; } = null!;

    public List<CrearFotoRequest> Fotos { get; set; } = new();

    public List<CrearRedSocialRequest> RedesSociales { get; set; } = new();

    public List<CrearFamiliarRequest> Familiares { get; set; } = new();
}