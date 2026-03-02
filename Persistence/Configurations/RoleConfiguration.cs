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
            .HasMaxLength(50);

        // Relación 1:N con Usuarios (Un rol tiene muchos usuarios)
        builder.HasMany(r => r.Usuarios)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación N:N (vía intermedia) con Módulos
        builder.HasMany(r => r.Permisos)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId);
            
    }
}