using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        // --- Datos Personales ---
        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Apellidos)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Correo)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.Correo)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Celular)
            .HasMaxLength(20);

        builder.Property(u => u.Imagen)
            .HasMaxLength(500);

        // --- Confirmación y Seguridad ---
        builder.Property(u => u.CorreoConfirmado)
            .HasDefaultValue(false);

        builder.Property(u => u.TokenConfirmacionCorreo)
            .HasMaxLength(250);

        builder.Property(u => u.IntentosFallidos)
            .HasDefaultValue(0);

        // --- Auditoría ---
        builder.Property(u => u.Activo)
            .HasDefaultValue(true);

        builder.Property(u => u.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        // --- Relaciones ---
        
        // Relación con Roles (Ya la tenías)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // NOTA: No es obligatorio configurar la relación con UsuarioSesiones aquí 
        // porque ya la configuramos en 'UsuarioSesionConfiguration'. 
        // EF Core es inteligente y entiende la relación bidireccional.
    }
}