using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Models;
using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

public sealed class TorneioService(IDbContextFactory<PortalCopaDbContext> dbFactory) : ITorneioService
{
    /// <summary>Países-sede na ordem de exibição, com nome e código FIFA.</summary>
    private static readonly (PaisSede Pais, string Nome, string CodigoFifa)[] PaisesSede =
    [
        (PaisSede.EstadosUnidos, "Estados Unidos", "USA"),
        (PaisSede.Canada, "Canadá", "CAN"),
        (PaisSede.Mexico, "México", "MEX"),
    ];

    public async Task<ResumoTorneioDto> ObterResumoAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Um único DbContext não admite consultas simultâneas; as contagens são sequenciais.
        var selecoes = await db.Selecoes.CountAsync(cancellationToken);
        var grupos = await db.Grupos.CountAsync(cancellationToken);
        var jogos = await db.Jogos.CountAsync(cancellationToken);
        var estadios = await db.Estadios.CountAsync(cancellationToken);

        return new ResumoTorneioDto(selecoes, grupos, jogos, estadios);
    }

    public async Task<IReadOnlyList<PaisSedeDto>> ObterPaisesSedeAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var estadiosPorPais = await db.Estadios
            .AsNoTracking()
            .GroupBy(e => e.Pais)
            .Select(g => new { Pais = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Pais, x => x.Quantidade, cancellationToken);

        return PaisesSede
            .Select(p => new PaisSedeDto(p.Nome, p.CodigoFifa, estadiosPorPais.GetValueOrDefault(p.Pais)))
            .ToList();
    }
}
