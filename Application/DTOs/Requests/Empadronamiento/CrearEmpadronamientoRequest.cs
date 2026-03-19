namespace EmpadronamientoBackend.Application.DTOs.Requests;
public class CrearEmpadronamientoRequest
{
    public DateTime Fecha { get; set; }

    public TimeSpan Hora { get; set; }

    public required string CRPN { get; set; }

    public string NarrativaHechos { get; set; } = string.Empty;

    /// <summary>
    /// Lugar donde ocurre el empadronamiento
    /// </summary>
    public CrearLugarEmpadronamientoRequest Lugar { get; set; } = null!;

    /// <summary>
    /// Personas registradas
    /// </summary>
    public List<CrearPersonaEmpadronamientoRequest> Personas { get; set; } = new();
}