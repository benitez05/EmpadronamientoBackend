namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class ModuloPermisoResponseDto
{
    public int ModuloId { get; set; }
    public string Nombre { get; set; } = null!;
    public string K { get; set; } = null!;
    public string Color { get; set; } = null!;
    public bool Multinivel { get; set; }
    
    /// <summary>
    /// 0: Sin acceso, 1: Leer, 2: Escribir, 3: Borrar
    /// </summary>
    public int NivelAcceso { get; set; }
}