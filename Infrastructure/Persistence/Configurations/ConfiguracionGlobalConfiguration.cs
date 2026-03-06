using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConfiguracionGlobalConfiguration : IEntityTypeConfiguration<ConfiguracionGlobal>
{
    public void Configure(EntityTypeBuilder<ConfiguracionGlobal> builder)
    {
        builder.ToTable("ConfiguracionGlobal");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreSistema).HasMaxLength(100);
        builder.Property(c => c.ZonaHoraria).HasMaxLength(100);
        
    }
}