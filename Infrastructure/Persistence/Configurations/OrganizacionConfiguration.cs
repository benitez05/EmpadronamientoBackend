using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizacionConfiguration : IEntityTypeConfiguration<Organizacion>
{
    public void Configure(EntityTypeBuilder<Organizacion> builder)
    {
        builder.ToTable("Organizaciones");
        builder.HasKey(o => o.Id);

        // Nombre: Obligatorio y con límite
        builder.Property(o => o.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        // Email de Contacto: DEBE ser obligatorio y único
        builder.Property(o => o.EmailContacto)
            .IsRequired() // <--- Hazlo obligatorio
            .HasMaxLength(150);

        // Índice único para el correo (evita duplicidad de cuentas empresariales)
        builder.HasIndex(o => o.EmailContacto)
            .IsUnique();

        builder.Property(o => o.Telefono)
            .HasMaxLength(20);

        // Configuración de campos de estado (opcional pero recomendado)
        builder.Property(o => o.Activa)
            .HasDefaultValue(true);

        // Relación 1:N con Plan
        builder.HasOne(o => o.Plan)
            .WithMany()
            .HasForeignKey(o => o.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Usuarios (1:N)
        builder.HasMany(o => o.Usuarios)
            .WithOne(u => u.Organizacion)
            .HasForeignKey(u => u.OrganizacionId)
            .OnDelete(DeleteBehavior.Cascade); // Si borras la org, mueren sus usuarios
    }
}