using BenitezLabs.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class CatalogoItemConfiguration : IEntityTypeConfiguration<CatalogoItem>
{
    public void Configure(EntityTypeBuilder<CatalogoItem> builder)
    {
        builder.ToTable("CatalogoItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Codigo)
            .HasMaxLength(50);

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.CatalogoId, x.Orden });

        builder.HasOne(x => x.Catalogo)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}