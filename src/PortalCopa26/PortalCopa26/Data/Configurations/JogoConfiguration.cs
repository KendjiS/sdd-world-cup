using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class JogoConfiguration : IEntityTypeConfiguration<Jogo>
{
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        builder.ToTable("Jogos");
        builder.HasIndex(j => j.Numero).IsUnique();
        builder.HasIndex(j => j.DataHoraUtc);
        builder.Property(j => j.Fase).HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.Rotulo).HasMaxLength(40);
        builder.Property(j => j.DescricaoMandante).HasMaxLength(60);
        builder.Property(j => j.DescricaoVisitante).HasMaxLength(60);

        builder.HasOne(j => j.Grupo)
            .WithMany(g => g.Jogos)
            .HasForeignKey(j => j.GrupoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Estadio)
            .WithMany(e => e.Jogos)
            .HasForeignKey(j => j.EstadioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.SelecaoMandante)
            .WithMany()
            .HasForeignKey(j => j.SelecaoMandanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.SelecaoVisitante)
            .WithMany()
            .HasForeignKey(j => j.SelecaoVisitanteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
