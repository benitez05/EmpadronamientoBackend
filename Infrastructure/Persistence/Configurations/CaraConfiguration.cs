using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Infrastructure.Persistence.Configurations;

public class CaraConfiguration : IEntityTypeConfiguration<Cara>
{
    public void Configure(EntityTypeBuilder<Cara> builder)
    {
        builder.ToTable("Caras");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FaceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.S3Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Confidence)
            .IsRequired();

        // El BoundingBox se guarda como string (JSON)
        builder.Property(c => c.BoundingBox)
            .IsRequired(false);

        // Relación con Foto
        builder.HasOne(c => c.Foto)
            .WithMany()
            .HasForeignKey(c => c.IdFoto)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.FaceId)
            .IsUnique(); // Vital para evitar duplicados de vectores en AWS

        builder.HasIndex(c => c.OrganizacionId);
        builder.HasIndex(c => c.S3Key);
    }
}