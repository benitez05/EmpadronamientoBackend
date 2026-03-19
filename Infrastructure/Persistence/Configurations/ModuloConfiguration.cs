using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Persistence.Configurations;

public class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable("Modulos");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.K)
            .IsRequired()
            .HasMaxLength(10); 

        builder.HasIndex(m => m.K)
            .IsUnique();

        // --- CONFIGURACIÓN DEL COLOR ---
        builder.Property(m => m.Color)
            .IsRequired()
            .HasMaxLength(7) // Para guardar "#FFFFFF"
            .HasDefaultValue("#3B82F6"); // Azul por defecto (Tailwind Blue 500)

        // --- CONFIGURACIÓN DEL ICONO ---
        builder.Property(m => m.Icono)
            .HasMaxLength(150); 
    }
}