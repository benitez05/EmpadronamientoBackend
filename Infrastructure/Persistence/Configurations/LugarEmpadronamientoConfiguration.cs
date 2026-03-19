using BenitezLabs.Domain.Entities.Empadronamientos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class LugarEmpadronamientoConfiguration : IEntityTypeConfiguration<LugarEmpadronamiento>
{
    public void Configure(EntityTypeBuilder<LugarEmpadronamiento> builder)
    {
        builder.ToTable("LugaresEmpadronamiento");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Calle).HasMaxLength(200);
        builder.Property(x => x.NumeroExterior).HasMaxLength(50);
        builder.Property(x => x.NumeroInterior).HasMaxLength(50);
        builder.Property(x => x.Colonia).HasMaxLength(150);
        builder.Property(x => x.Municipio).HasMaxLength(150);
        builder.Property(x => x.Estado).HasMaxLength(150);
        builder.Property(x => x.Referencia).HasMaxLength(500);
        // Latitud: Máximo 90.00000000 (2 enteros + 8 decimales = 10 dígitos)
        builder.Property(x => x.Latitud)
            .HasPrecision(10, 8);

        // Longitud: Máximo 180.00000000 (3 enteros + 8 decimales = 11 dígitos)
        builder.Property(x => x.Longitud)
            .HasPrecision(11, 8);

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}