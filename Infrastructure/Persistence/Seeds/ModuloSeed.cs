using Microsoft.EntityFrameworkCore;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Seeds;

public static class ModulosSeed
{
    public static void Seed(ModelBuilder modelBuilder, DateTime fecha, string sys, string ip, string dev)
    {
        modelBuilder.Entity<Modulo>().HasData(
            new Modulo { Id = 1, Nombre = "Usuarios", K = "u" },
            new Modulo { Id = 2, Nombre = "Roles", K = "r" },
            new Modulo { Id = 3, Nombre = "Organizaciones", K = "o" },
            new Modulo { Id = 4, Nombre = "Configuración", K = "c" },
            new Modulo { Id = 5, Nombre = "Empadronamiento", K = "e" },
            new Modulo { Id = 6, Nombre = "Busqueda", K = "b" },
            new Modulo { Id = 7, Nombre = "Estadisticas", K = "t" }
        );

        modelBuilder.Entity<OrganizacionModulo>().HasData(
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 1, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 2, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 3, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 4, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 5, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 6, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 7, Activo = true, FechaActivacion = fecha, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev }
        );
    }
}