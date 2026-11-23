using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class EstadioConfiguration : IEntityTypeConfiguration<Estadio>
{
    public void Configure(EntityTypeBuilder<Estadio> builder)
    {
        builder.ToTable("Estadios");
        builder.Property(e => e.Nome).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Cidade).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Pais).HasConversion<string>().HasMaxLength(20);
    }
}
