using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

// 1. Namespace actualizado a la nueva ruta
namespace EmpadronamientoBackend.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ============================
    // TABLAS (DbSets)
    // ============================

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<RolePermiso> RolesPermisos => Set<RolePermiso>();
    
    // 2. Agregamos la nueva tabla de sesiones
    public DbSet<UsuarioSesion> UsuarioSesiones => Set<UsuarioSesion>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Esto aplicará automáticamente UsuarioSesionConfiguration.cs
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // SEED DE ROLES
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Nombre = "SuperAdmin" },
            new Role { Id = 2, Nombre = "Usuario" }
        );

        // SEED DE MÓDULOS
        modelBuilder.Entity<Modulo>().HasData(
            new Modulo { Id = 1, Nombre = "Usuarios", K = "u" },
            new Modulo { Id = 2, Nombre = "Roles", K = "r" },
            new Modulo { Id = 3, Nombre = "Modulos", K = "m" }
        );

        // SEED DE PERMISOS
        // OJO: Cambié el RoleId a 1 (SuperAdmin) para que los niveles 3 tengan sentido.
        // Si quieres que el usuario normal también tenga acceso, puedes agregar más líneas.
        modelBuilder.Entity<RolePermiso>().HasData(
            new RolePermiso { RoleId = 2, ModuloId = 1, Lvl = 1 }, 
            new RolePermiso { RoleId = 2, ModuloId = 2, Lvl = 1 }, 
            new RolePermiso { RoleId = 2, ModuloId = 3, Lvl = 1 }
        );
    }
}