using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Seeds;

public static class SeguridadSeed
{
    public static void Seed(ModelBuilder modelBuilder, DateTime fecha, string sys, string ip, string dev)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Nombre = "SuperAdmin", OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev },
            new Role { Id = 2, Nombre = "Usuario", OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fecha, IpCreacion = ip, DispositivoCreacion = dev }
        );

        modelBuilder.Entity<RolePermiso>().HasData(
            new RolePermiso { RoleId = 2, ModuloId = 1, Lvl = 1 },
            new RolePermiso { RoleId = 2, ModuloId = 2, Lvl = 1 }
        );

        var hasher = new PasswordHasher<Usuario>();

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Admin",
                Apellidos = "BenitezLabs",
                Correo = "admin@benitezlabs.com",
                PasswordHash = hasher.HashPassword(null!, "Admin123!"),
                OrganizacionId = 1,
                RoleId = 1,
                Tipo = 3,
                Activo = true,
                CorreoConfirmado = true,
                IntentosFallidos = 0,
                FechaCreacion = fecha,
                CreadoPor = sys,
                IpCreacion = ip,
                DispositivoCreacion = dev
            },
            new Usuario
            {
                Id = 2,
                Nombre = "Demo",
                Apellidos = "Operator",
                Correo = "user@benitezlabs.com",
                PasswordHash = hasher.HashPassword(null!, "Admin123!"),
                OrganizacionId = 1,
                RoleId = 2,
                Tipo = 1,
                Activo = true,
                CorreoConfirmado = true,
                IntentosFallidos = 0,
                FechaCreacion = fecha,
                CreadoPor = sys,
                IpCreacion = ip,
                DispositivoCreacion = dev
            }
        );
    }
}