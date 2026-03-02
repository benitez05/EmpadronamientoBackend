using BenitezLabs.Domain.Entities; // Ajusta a tu namespace real
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Persistence.Configurations;

/// <summary>
/// Configuración de la entidad Usuario usando Fluent API.
/// Separa las reglas de persistencia de la lógica de dominio.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // 1. Nombre de la tabla
        builder.ToTable("Usuarios");

        // 2. Llave Primaria
        builder.HasKey(u => u.Id);

        // 3. Propiedades Básicas
        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Apellidos)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Correo)
            .IsRequired()
            .HasMaxLength(150);

        // Índice único para el correo (No queremos duplicados, pendejín)
        builder.HasIndex(u => u.Correo)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Celular)
            .HasMaxLength(20);

        builder.Property(u => u.Imagen)
            .HasMaxLength(500); // URL de la imagen

        // 4. Configuración de Roles (Relación 1:N)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // No borres el Rol si tiene usuarios

        // 5. Seguridad (Tokens y Bloqueos)
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.TokenConfirmacionCorreo)
            .HasMaxLength(250);

        builder.Property(u => u.IntentosFallidos)
            .HasDefaultValue(0);

        // 6. Auditoría y Fechas
        builder.Property(u => u.Activo)
            .HasDefaultValue(true);

        builder.Property(u => u.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)"); // Depende de tu BD (MySQL/Postgres)

        builder.Property(u => u.FechaActualizacion)
            .IsRequired(false);
    }
}