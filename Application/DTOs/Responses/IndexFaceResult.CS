namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class IndexFaceResult
{
    public string? FaceId { get; set; }
    
    // Aquí mantenemos a salvo el BoundingBox para el Front-end
    public string? BoundingBoxJson { get; set; }
    
    public float? Confidence { get; set; }

    // Propiedad calculada para saber si funcionó fácilmente
    public bool Exito => !string.IsNullOrEmpty(FaceId);
}