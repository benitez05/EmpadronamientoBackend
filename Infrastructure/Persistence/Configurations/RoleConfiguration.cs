using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        // --- NUEVO: MULTI-TENANT ---
        builder.Property(r => r.OrganizacionId)
            .IsRequired();

        builder.HasOne(r => r.Organizacion)
            .WithMany() // Una organización tiene muchos roles
            .HasForeignKey(r => r.OrganizacionId)
            .OnDelete(DeleteBehavior.Cascade); // Si muere la Org, mueren sus roles

        // ÍNDICE ÚNICO: Nombre de rol único POR organización
        builder.HasIndex(r => new { r.Nombre, r.OrganizacionId })
            .IsUnique();

        // --- NUEVO: AUDITORÍA (Strings) ---
        builder.Property(r => r.CreadoPor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.ActualizadoPor)
            .HasMaxLength(150);

        // --- TUS RELACIONES EXISTENTES (Se mantienen) ---
        
        // Relación 1:N con Usuarios
        builder.HasMany(r => r.Usuarios)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Permisos
        builder.HasMany(r => r.Permisos)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId);
    }
}