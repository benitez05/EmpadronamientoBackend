using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class UsuarioSesionConfiguration : IEntityTypeConfiguration<UsuarioSesion>
{
    public void Configure(EntityTypeBuilder<UsuarioSesion> builder)
    {
        builder.ToTable("UsuarioSesiones");

        builder.HasKey(s => s.Id);

        // El JTI es el folio único del JWT, lo hacemos indexado para búsquedas rápidas
        builder.Property(s => s.Jti)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Jti)
            .IsUnique();

        builder.Property(s => s.RefreshToken)
            .IsRequired()
            .HasMaxLength(255);

        // Información del dispositivo (User-Agent)
        builder.Property(s => s.DeviceInfo)
            .HasMaxLength(255);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(50);

        // Configuración de la relación 1:N (Un usuario, muchas sesiones)
        builder.HasOne(s => s.Usuario)
            .WithMany(u => u.Sesiones)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade); // Si borras al usuario, se mueren sus sesiones
    }
}