using BenitezLabs.Domain.Entities.Empadronamientos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class EmpadronamientoConfiguration : IEntityTypeConfiguration<Empadronamiento>
{
    public void Configure(EntityTypeBuilder<Empadronamiento> builder)
    {
        builder.ToTable("Empadronamientos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NarrativaHechos)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.Folio)
            .HasMaxLength(100);

        builder.Property(x => x.CRP)
            .HasMaxLength(100);

        builder.HasOne(x => x.UsuarioResponsable)
            .WithMany()
            .HasForeignKey(x => x.UsuarioResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lugar)
            .WithMany()
            .HasForeignKey(x => x.LugarEmpadronamientoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Personas)
            .WithOne(x => x.Empadronamiento)
            .HasForeignKey(x => x.EmpadronamientoId);
    }
}