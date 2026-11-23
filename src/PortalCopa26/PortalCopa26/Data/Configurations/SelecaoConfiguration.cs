using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class SelecaoConfiguration : IEntityTypeConfiguration<Selecao>
{
    public void Configure(EntityTypeBuilder<Selecao> builder)
    {
        builder.ToTable("Selecoes");
        builder.Property(s => s.CodigoFifa).HasMaxLength(3).IsRequired();
        builder.Property(s => s.Nome).HasMaxLength(60).IsRequired();
        builder.Property(s => s.Tecnico).HasMaxLength(80).IsRequired();
        builder.HasIndex(s => s.CodigoFifa).IsUnique();
        builder.Ignore(s => s.BandeiraUrl);

        builder.HasOne(s => s.Grupo)
            .WithMany(g => g.Selecoes)
            .HasForeignKey(s => s.GrupoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
