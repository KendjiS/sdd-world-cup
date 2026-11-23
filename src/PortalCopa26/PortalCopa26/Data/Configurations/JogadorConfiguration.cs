using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Configurations;

public class JogadorConfiguration : IEntityTypeConfiguration<Jogador>
{
    public void Configure(EntityTypeBuilder<Jogador> builder)
    {
        builder.ToTable("Jogadores");
        builder.Property(j => j.Nome).HasMaxLength(100).IsRequired();
        builder.Property(j => j.Posicao).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(j => j.Selecao)
            .WithMany(s => s.Jogadores)
            .HasForeignKey(j => j.SelecaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
