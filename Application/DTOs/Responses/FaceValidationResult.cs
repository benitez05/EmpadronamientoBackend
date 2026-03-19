namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class FaceValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
}