using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class AsignarPermisoRequest
{
    /// <summary>ID del Rol al que se le asignará el permiso</summary>
    [DefaultValue(1)]
    public int RoleId { get; set; }

    /// <summary>ID del Módulo al que tendrá acceso</summary>
    [DefaultValue(1)]
    public int ModuloId { get; set; }

    /// <summary>Nivel de jerarquía (1: Leer, 2: Escribir, 3: Borrar)</summary>
    [DefaultValue(3)]
    public int Lvl { get; set; }
}