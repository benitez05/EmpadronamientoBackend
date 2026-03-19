using Microsoft.EntityFrameworkCore;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Seeds;

public static class SistemaSeed
{
    public static void Seed(ModelBuilder modelBuilder, DateTime fecha, string sys, string ip, string dev)
    {
        // =========================
        // PLAN
        // =========================
        modelBuilder.Entity<Plan>().HasData(
            new Plan
            {
                Id = 1,
                Nombre = "Plan Maestro / Enterprise",
                Precio = 0,
                MaxUsuarios = 9999,
                Activo = true,
                FechaCreacion = fecha
            }
        );

        // =========================
        // ORGANIZACIÓN
        // =========================
        modelBuilder.Entity<Organizacion>().HasData(
            new Organizacion
            {
                Id = 1,
                Nombre = "BenitezLabs Admin",
                EmailContacto = "admin@benitezlabs.com",
                Descripcion = "Organización Maestra del Sistema",
                PlanId = 1,
                Activa = true,
                FechaVencimiento = fecha.AddYears(50),
                FechaCreacion = fecha,
                CreadoPor = sys,
                IpCreacion = ip,
                DispositivoCreacion = dev
            }
        );

        // =========================
        // CONFIG GLOBAL
        // =========================
        modelBuilder.Entity<ConfiguracionGlobal>().HasData(
            new ConfiguracionGlobal
            {
                Id = 1,
                NombreSistema = "BenitezLabs Enterprise",
                OrganizacionMaestraId = 1,
                ModoAdminGlobalActivo = true,
                MaxIntentosLoginFallidos = 5,
                MinutosBloqueo = 15,
                ZonaHoraria = "Central Standard Time",
                FechaCreacion = fecha,
                CreadoPor = sys,
                IpCreacion = ip,
                DispositivoCreacion = dev
            }
        );
    }
}