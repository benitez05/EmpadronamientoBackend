using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using BenitezLabs.Domain.Common;
using Microsoft.AspNetCore.Identity;
using BenitezLabs.Domain.Entities.Empadronamientos;
using BenitezLabs.Domain.Entities.Catalogos;
using EmpadronamientoBackend.Infrastructure.Persistence.Seeds;

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
    // DBSETS
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

    public DbSet<Empadronamiento> Empadronamientos => Set<Empadronamiento>();
    public DbSet<EmpadronamientoPersona> EmpadronamientoPersonas => Set<EmpadronamientoPersona>();
    public DbSet<LugarEmpadronamiento> LugaresEmpadronamiento => Set<LugarEmpadronamiento>();

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<DireccionPersona> DireccionesPersona => Set<DireccionPersona>();
    public DbSet<Familiar> Familiares => Set<Familiar>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<RedSocial> RedesSociales => Set<RedSocial>();
    public DbSet<Cara> Caras => Set<Cara>();

    public DbSet<Catalogo> Catalogos => Set<Catalogo>();
    public DbSet<CatalogoItem> CatalogoItems => Set<CatalogoItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        base.OnConfiguring(optionsBuilder);
    }

    // ============================================================
    // AUDITORÍA AUTOMÁTICA + MULTI-TENANT
    // ============================================================
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEmail = _currentUserService.Email ?? "Sistema";
        var userIp = _currentUserService.IpAddress ?? "127.0.0.1";
        var dispositivo = _currentUserService.Dispositivo ?? "Desconocido";
        var currentOrgId = _currentUserService.OrganizacionId ;

        foreach (var entry in ChangeTracker.Entries<AuditoriaEntidad>())
        {
            if (entry.State == EntityState.Added)
            {
                var shadowProp = entry.Entity.GetType().GetProperty("OrganizacionId");

                if (shadowProp != null)
                {
                    int val = (int)shadowProp.GetValue(entry.Entity)!;
                    if (val == 0 && currentOrgId != 0)
                    {
                        shadowProp.SetValue(entry.Entity, currentOrgId);
                    }
                }

                entry.Entity.FechaCreacion = DateTime.UtcNow;
                entry.Entity.CreadoPor = userEmail;
                entry.Entity.IpCreacion = userIp;
                entry.Entity.DispositivoCreacion = dispositivo;
            }
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

        // ==========================================
        // 🔥 SEEDS CENTRALIZADOS
        // ==========================================
        modelBuilder.ApplySeeds();

        // ==========================================
        // CONFIGURACIÓN GLOBAL (AUDITORÍA + FILTROS)
        // ==========================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;

            // Auditoría
            if (typeof(AuditoriaEntidad).IsAssignableFrom(type))
            {
                modelBuilder.Entity(type).Property("CreadoPor").HasMaxLength(150);
                modelBuilder.Entity(type).Property("ActualizadoPor").HasMaxLength(150);
                modelBuilder.Entity(type).Property("IpCreacion").HasMaxLength(50);
                modelBuilder.Entity(type).Property("IpUltimaActualizacion").HasMaxLength(50);
                modelBuilder.Entity(type).Property("DispositivoCreacion").HasMaxLength(250);
                modelBuilder.Entity(type).Property("DispositivoUltimaActualizacion").HasMaxLength(250);
            }

            // Multi-tenant filter
            var property = entityType.FindProperty("OrganizacionId");

            if (property != null && property.ClrType == typeof(int) && type != typeof(Organizacion))
            {
                var parameter = Expression.Parameter(type, "e");

                var userOrgIdProp = Expression.Property(
                    Expression.Constant(_currentUserService),
                    nameof(ICurrentUserService.OrganizacionId)
                );

                var convertedUserOrgId = Expression.Convert(userOrgIdProp, typeof(int));

                var tipoProp = Expression.Property(
                    Expression.Constant(_currentUserService),
                    "Tipo"
                );

                var eqOrg = Expression.Equal(Expression.Property(parameter, "OrganizacionId"), convertedUserOrgId);
                var isGod = Expression.Equal(tipoProp, Expression.Constant(4));

                var filterBody = Expression.OrElse(eqOrg, isGod);
                var filterExpr = Expression.Lambda(filterBody, parameter);

                modelBuilder.Entity(type).HasQueryFilter(filterExpr);
            }
        }
    }
}