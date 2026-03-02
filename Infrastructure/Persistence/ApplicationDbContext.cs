using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BenitezLabs.Persistence;

/// <summary>
/// DbContext principal del sistema BenitezLabs.
/// Maneja el acceso a la base de datos y la configuración de las entidades.
/// </summary>
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

    /// <summary>
    /// Configuración global del contexto.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ACTIVA EL LAZY LOADING (Requiere el paquete de Proxies)
        // Esto hace que las propiedades 'virtual' se llenen solitas.
        optionsBuilder.UseLazyLoadingProxies();

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Aplica automáticamente todas las configuraciones Fluent API del assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Escanea este proyecto en busca de archivos que hereden de IEntityTypeConfiguration
        // y aplica las reglas que pusimos en UsuarioConfiguration, RoleConfiguration, etc.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // SEED DE ROLES
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Nombre = "SuperAdmin" },
            new Role { Id = 2, Nombre = "Usuario" }
        );

    // 2. Crear los Módulos (Keys que usas en el Token)
    modelBuilder.Entity<Modulo>().HasData(
        new Modulo { Id = 1, Nombre = "Usuarios", K = "u" },
        new Modulo { Id = 2, Nombre = "Roles", K = "r" },
        new Modulo { Id = 3, Nombre = "Modulos", K = "m" }
    );

    // 3. ASIGNAR LOS PERMISOS (La unión)
    // Aquí le dices: El Rol 1 en el Módulo 1 tiene Nivel 3
    modelBuilder.Entity<RolePermiso>().HasData(
        new RolePermiso { RoleId = 2, ModuloId = 1, Lvl = 3 }, // Admin puede todo en Usuarios
        new RolePermiso { RoleId = 2, ModuloId = 2, Lvl = 3 }, // Admin puede todo en Roles
        new RolePermiso { RoleId = 2, ModuloId = 3, Lvl = 3 }  // Admin puede todo en Módulos
    );
    }
}