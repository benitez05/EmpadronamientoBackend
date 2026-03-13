using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

public class ConfiguracionGlobal : AuditoriaEntidad
{
    public int Id { get; set; }
    public string NombreSistema { get; set; } = "BenitezLabs";

    // --- CONFIGURACIÓN MAESTRA ---
    public int OrganizacionMaestraId { get; set; } 

    // --- CONTROL "MODO DIOS" ---
    public bool ModoAdminGlobalActivo { get; set; } 

    // --- SEGURIDAD Y BLOQUEO DE CUENTAS ---
    // Cuántas veces puede fallar antes de que el sistema le cierre la puerta
    public int MaxIntentosLoginFallidos { get; set; } = 5;

    // Tiempo de castigo en minutos (ej. 15, 30, 60)
    public int MinutosBloqueo { get; set; } = 15;

    // --- LOCALIZACIÓN ---
    public string ZonaHoraria { get; set; } = "Central Standard Time";

}

