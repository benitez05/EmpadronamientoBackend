using Microsoft.EntityFrameworkCore;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Seeds;

public static class SeedExtensions
{
    public static void ApplySeeds(this ModelBuilder modelBuilder)
    {
        var fecha = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sys = "System_Seed";
        var ip = "127.0.0.1";
        var dev = "Console_Setup";

        // 🔥 ORDEN CORRECTO (DEPENDENCIAS)
        SistemaSeed.Seed(modelBuilder, fecha, sys, ip, dev);
        ModulosSeed.Seed(modelBuilder, fecha, sys, ip, dev);
        SeguridadSeed.Seed(modelBuilder, fecha, sys, ip, dev);
        CatalogosSeed.Seed(modelBuilder, fecha, sys, ip, dev);
    }
}