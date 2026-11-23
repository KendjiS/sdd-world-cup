using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class RankingFifaConfiguration : IEntityTypeConfiguration<RankingFifa>
{
    public void Configure(EntityTypeBuilder<RankingFifa> builder)
    {
        builder.ToTable("RankingFifa");
        builder.Property(r => r.NomeSelecao).HasMaxLength(60).IsRequired();
        builder.Property(r => r.Pontos).HasPrecision(7, 2);
        builder.HasIndex(r => r.Posicao).IsUnique();

        // Índice único simples: o SQLite já aceita vários NULL em um índice único.
        builder.HasIndex(r => r.SelecaoId).IsUnique().HasFilter(null);

        builder.HasOne(r => r.Selecao)
            .WithMany()
            .HasForeignKey(r => r.SelecaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
