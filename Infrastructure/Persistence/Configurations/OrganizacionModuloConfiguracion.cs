using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizacionModuloConfiguration : IEntityTypeConfiguration<OrganizacionModulo>
{
    public void Configure(EntityTypeBuilder<OrganizacionModulo> builder)
    {
        builder.ToTable("OrganizacionModulos");

        // DEFINIR LLAVE COMPUESTA
        builder.HasKey(om => new { om.OrganizacionId, om.ModuloId });

        builder.HasOne(om => om.Organizacion)
            .WithMany(o => o.ModulosContratados)
            .HasForeignKey(om => om.OrganizacionId);

        builder.HasOne(om => om.Modulo)
            .WithMany(m => m.OrganizacionesConAcceso)
            .HasForeignKey(om => om.ModuloId);
        
    }
}