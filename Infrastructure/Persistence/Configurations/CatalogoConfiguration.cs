using BenitezLabs.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("Catalogos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Clave)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.Clave, x.OrganizacionId })
            .IsUnique();

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Catalogo)
            .HasForeignKey(i => i.CatalogoId);
    }
}