using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Infrastructure.Persistence.Configurations;

public class FotoConfiguration : IEntityTypeConfiguration<Foto>
{
    public void Configure(EntityTypeBuilder<Foto> builder)
    {
        builder.ToTable("Fotos");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.Tipo)
            .HasMaxLength(50);

        builder.Property(f => f.Descripcion)
            .HasMaxLength(255);

        // Relaciones
        builder.HasOne(f => f.Persona)
            .WithMany() 
            .HasForeignKey(f => f.IdPersona)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Empadronamiento)
            .WithMany()
            .HasForeignKey(f => f.IdEmpadronamiento)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices de búsqueda
        builder.HasIndex(f => f.OrganizacionId);
        builder.HasIndex(f => f.S3Key);
        builder.HasIndex(f => f.IdPersona);
    }
}