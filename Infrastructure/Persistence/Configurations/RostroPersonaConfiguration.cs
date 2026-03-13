using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class RostroPersonaConfiguration : IEntityTypeConfiguration<RostroPersona>
{
    public void Configure(EntityTypeBuilder<RostroPersona> builder)
    {
        builder.ToTable("RostrosPersona");

        builder.HasKey(x => x.IdRostro);

        builder.Property(x => x.ImageUri)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FaceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ImageId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.Rostros)
            .HasForeignKey(x => x.PersonaId);

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}