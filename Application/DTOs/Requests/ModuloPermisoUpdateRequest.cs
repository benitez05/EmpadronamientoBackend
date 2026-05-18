namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class ModuloPermisoUpdateRequestDto
{
    public int ModuloId { get; set; }
    
    /// <summary>
    /// 0: Sin acceso (Eliminar), 1: Leer, 2: Escribir, 3: Borrar
    /// </summary>
    public int NivelAcceso { get; set; }
}