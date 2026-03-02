namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class PermisoMatrixResponse
{
    public string Rol { get; set; } = null!;
    public string Modulo { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int Nivel { get; set; }
    public string DescripcionNivel { get; set; } = null!;
}