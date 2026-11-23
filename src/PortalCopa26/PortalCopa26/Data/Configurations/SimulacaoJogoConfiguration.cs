using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class SimulacaoJogoConfiguration : IEntityTypeConfiguration<SimulacaoJogo>
{
    public void Configure(EntityTypeBuilder<SimulacaoJogo> builder)
    {
        builder.ToTable("SimulacaoJogos");
        builder.HasIndex(sj => new { sj.SimulacaoId, sj.JogoId }).IsUnique();

        builder.HasOne(sj => sj.Simulacao)
            .WithMany(s => s.Jogos)
            .HasForeignKey(sj => sj.SimulacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sj => sj.Jogo)
            .WithMany()
            .HasForeignKey(sj => sj.JogoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
