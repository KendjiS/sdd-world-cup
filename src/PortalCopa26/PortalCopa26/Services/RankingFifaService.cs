using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

public sealed class RankingFifaService(IDbContextFactory<PortalCopaDbContext> dbFactory) : IRankingFifaService
{
    public async Task<IReadOnlyList<RankingItemDto>> ObterTopAsync(int quantidade, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidade);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.RankingFifa
            .AsNoTracking()
            .OrderBy(r => r.Posicao)
            .Take(quantidade)
            .Select(r => new RankingItemDto(
                r.Posicao,
                r.NomeSelecao,
                r.Selecao != null ? r.Selecao.CodigoFifa : null,
                r.Pontos))
            .ToListAsync(cancellationToken);
    }
}
