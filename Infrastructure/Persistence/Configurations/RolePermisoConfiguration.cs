using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Persistence.Configurations;

public class RolePermisoConfiguration : IEntityTypeConfiguration<RolePermiso>
{
    public void Configure(EntityTypeBuilder<RolePermiso> builder)
    {
        builder.ToTable("RolesPermisos");

        // LLAVE COMPUESTA: No puede haber un mismo Rol con el mismo Módulo dos veces
        builder.HasKey(rp => new { rp.RoleId, rp.ModuloId });

        builder.Property(rp => rp.Lvl)
            .IsRequired()
            .HasComment("Nivel de acceso: 1-Leer, 2-Escribir, 3-Borrar");

        // Relación con Role
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.Permisos)
            .HasForeignKey(rp => rp.RoleId);

        // Relación con Modulo
        builder.HasOne(rp => rp.Modulo)
            .WithMany(m => m.RolePermisos)
            .HasForeignKey(rp => rp.ModuloId);
    }
}