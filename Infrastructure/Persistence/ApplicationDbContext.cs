using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using BenitezLabs.Domain.Common;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EmpadronamientoBackend.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ============================
    // TABLAS (DbSets)
    // ============================
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<RolePermiso> RolesPermisos => Set<RolePermiso>();
    public DbSet<UsuarioSesion> UsuarioSesiones => Set<UsuarioSesion>();
    public DbSet<Organizacion> Organizaciones => Set<Organizacion>();
    public DbSet<OrganizacionModulo> OrganizacionModulos => Set<OrganizacionModulo>();
    public DbSet<ConfiguracionGlobal> ConfiguracionesGlobales => Set<ConfiguracionGlobal>();
    public DbSet<Plan> Planes => Set<Plan>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        base.OnConfiguring(optionsBuilder);
    }

    // ============================================================
    // AUDITORÍA AUTOMÁTICA
    // ============================================================
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEmail = _currentUserService.Email ?? "Sistema";
        var userIp = _currentUserService.IpAddress ?? "127.0.0.1";
        var dispositivo = _currentUserService.Dispositivo ?? "Desconocido";
        // Obtenemos el ID de la organización del token
        var currentOrgId = _currentUserService.OrganizacionId ?? 0;

        foreach (var entry in ChangeTracker.Entries<AuditoriaEntidad>())
        {
            // 1. ASIGNACIÓN AUTOMÁTICA DE MULTI-TENANT AL CREAR
            if (entry.State == EntityState.Added)
            {
                // Buscamos si la entidad tiene la propiedad OrganizacionId
                var shadowProp = entry.Entity.GetType().GetProperty("OrganizacionId");

                // Si tiene la propiedad y el valor actual es 0, le clavamos el del token
                if (shadowProp != null)
                {
                    int val = (int)shadowProp.GetValue(entry.Entity)!;
                    if (val == 0 && currentOrgId != 0)
                    {
                        shadowProp.SetValue(entry.Entity, currentOrgId);
                    }
                }

                // Auditoría normal
                entry.Entity.FechaCreacion = DateTime.UtcNow;
                entry.Entity.CreadoPor = userEmail;
                entry.Entity.IpCreacion = userIp;
                entry.Entity.DispositivoCreacion = dispositivo;
            }
            // 2. ACTUALIZACIÓN
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaUltimaActualizacion = DateTime.UtcNow;
                entry.Entity.ActualizadoPor = userEmail;
                entry.Entity.IpUltimaActualizacion = userIp;
                entry.Entity.DispositivoUltimaActualizacion = dispositivo;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // 2. CONFIGURACIÓN AUTOMÁTICA (AUDITORÍA Y FILTROS)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;

            // A. Auditoría (Tamaños)
            if (typeof(AuditoriaEntidad).IsAssignableFrom(type))
            {
                modelBuilder.Entity(type).Property("CreadoPor").HasMaxLength(150);
                modelBuilder.Entity(type).Property("ActualizadoPor").HasMaxLength(150);
                modelBuilder.Entity(type).Property("IpCreacion").HasMaxLength(50);
                modelBuilder.Entity(type).Property("IpUltimaActualizacion").HasMaxLength(50);
                modelBuilder.Entity(type).Property("DispositivoCreacion").HasMaxLength(250);
                modelBuilder.Entity(type).Property("DispositivoUltimaActualizacion").HasMaxLength(250);
            }

            // B. Filtro Global Multi-tenant con BYPASS para NIVEL 4 (Modo Dios)
            var property = entityType.FindProperty("OrganizacionId");
            if (property != null && property.ClrType == typeof(int) && type != typeof(Organizacion))
            {
                var parameter = Expression.Parameter(type, "e");

                // 1. OrganizacionId del usuario (desde ICurrentUserService)
                var userOrgIdProp = Expression.Property(
                    Expression.Constant(_currentUserService),
                    nameof(ICurrentUserService.OrganizacionId)
                );
                var convertedUserOrgId = Expression.Convert(userOrgIdProp, typeof(int));

                // 2. Tipo de Usuario (desde ICurrentUserService)cd ..
                var tipoProp = Expression.Property(
                    Expression.Constant(_currentUserService),
                    "Tipo"
                );

                // Lógica SQL: WHERE (OrganizacionId == @UserOrgId OR @UserTipo == 4)
                var eqOrg = Expression.Equal(Expression.Property(parameter, "OrganizacionId"), convertedUserOrgId);
                var isGod = Expression.Equal(tipoProp, Expression.Constant(4));

                var filterBody = Expression.OrElse(eqOrg, isGod);
                var filterExpr = Expression.Lambda(filterBody, parameter);

                modelBuilder.Entity(type).HasQueryFilter(filterExpr);
            }
        }

        // ==========================================
        // SEEDS (DATOS INICIALES)
        // ==========================================
        var fechaSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sys = "System_Seed";
        var ipSeed = "127.0.0.1";
        var devSeed = "Console_Setup";

        modelBuilder.Entity<Plan>().HasData(new Plan { Id = 1, Nombre = "Plan Maestro / Enterprise", Precio = 0, MaxUsuarios = 9999, Activo = true, FechaCreacion = fechaSeed });

        // SOLUCIÓN AL ERROR: Se añade EmailContacto
        modelBuilder.Entity<Organizacion>().HasData(new Organizacion
        {
            Id = 1,
            Nombre = "BenitezLabs Admin",
            EmailContacto = "admin@benitezlabs.com", // <-- ESTA PROPIEDAD FALTABA
            Descripcion = "Organización Maestra del Sistema",
            PlanId = 1,
            Activa = true,
            FechaVencimiento = fechaSeed.AddYears(50),
            FechaCreacion = fechaSeed,
            CreadoPor = sys,
            IpCreacion = ipSeed,
            DispositivoCreacion = devSeed
        });
        modelBuilder.Entity<Modulo>().HasData(
            new Modulo { Id = 1, Nombre = "Usuarios", K = "u" },
            new Modulo { Id = 2, Nombre = "Roles", K = "r" },
            new Modulo { Id = 3, Nombre = "Organizaciones", K = "o" },
            new Modulo { Id = 4, Nombre = "Configuración", K = "c" }
        );

        modelBuilder.Entity<OrganizacionModulo>().HasData(
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 1, Activo = true, FechaActivacion = fechaSeed, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 2, Activo = true, FechaActivacion = fechaSeed, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 3, Activo = true, FechaActivacion = fechaSeed, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed },
            new OrganizacionModulo { OrganizacionId = 1, ModuloId = 4, Activo = true, FechaActivacion = fechaSeed, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Nombre = "SuperAdmin", OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed },
            new Role { Id = 2, Nombre = "Usuario", OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ipSeed, DispositivoCreacion = devSeed }
        );

        modelBuilder.Entity<RolePermiso>().HasData(
            new RolePermiso { RoleId = 2, ModuloId = 1, Lvl = 1 },
            new RolePermiso { RoleId = 2, ModuloId = 2, Lvl = 1 }
        );

        modelBuilder.Entity<ConfiguracionGlobal>().HasData(new ConfiguracionGlobal { Id = 1, NombreSistema = "BenitezLabs Enterprise", OrganizacionMaestraId = 1, ModoAdminGlobalActivo = true, MaxIntentosLoginFallidos = 5, MinutosBloqueo = 15, ZonaHoraria = "Central Standard Time", FechaCreacion = fechaSeed, CreadoPor = sys, IpCreacion = ipSeed, DispositivoCreacion = devSeed });

        var hasher = new PasswordHasher<Usuario>();
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nombre = "Admin", Apellidos = "BenitezLabs", Correo = "admin@benitezlabs.com", PasswordHash = hasher.HashPassword(null!, "Admin123!"), OrganizacionId = 1, RoleId = 1,Tipo = 3, Activo = true, CorreoConfirmado = true, IntentosFallidos = 0, FechaCreacion = fechaSeed, CreadoPor = sys, IpCreacion = ipSeed, DispositivoCreacion = devSeed },
            new Usuario { Id = 2, Nombre = "Demo", Apellidos = "Operator", Correo = "user@benitezlabs.com", PasswordHash = hasher.HashPassword(null!, "Admin123!"), OrganizacionId = 1, RoleId = 2, Tipo = 1, Activo = true, CorreoConfirmado = true, IntentosFallidos = 0, FechaCreacion = fechaSeed, CreadoPor = sys, IpCreacion = ipSeed, DispositivoCreacion = devSeed }
        );
    }
}