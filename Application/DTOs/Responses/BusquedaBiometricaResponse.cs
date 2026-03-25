namespace EmpadronamientoBackend.Application.DTOs.Responses.Busqueda;

/// <summary>
/// Respuesta ligera con los datos básicos de una coincidencia biométrica.
/// </summary>
public class BusquedaBiometricaBasicaResponse
{
    public int PersonaId { get; set; }
    
    public string NombreCompleto { get; set; } = string.Empty;
    
    public string Apodo { get; set; } = string.Empty;
    
    /// <summary>
    /// Porcentaje de coincidencia devuelto por AWS Rekognition (Ej. 98.5).
    /// </summary>
    public float Similitud { get; set; } 
    
    /// <summary>
    /// URL pública o firmada de la foto principal del rostro en S3.
    /// </summary>
    public string? FotoPrincipalUrl { get; set; }
    
    /// <summary>
    /// Folio del empadronamiento más reciente de esta persona.
    /// </summary>
    public string UltimoFolio { get; set; } = "SIN REGISTRO";
}