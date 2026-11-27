using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

public sealed class GrupoService(IDbContextFactory<PortalCopaDbContext> dbFactory) : IGrupoService
{
    public async Task<IReadOnlyList<GrupoResumoDto>> ObterGruposAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var grupos = await db.Grupos
            .AsNoTracking()
            .OrderBy(g => g.Codigo)
            .Select(g => new
            {
                g.Codigo,
                // Ordem do seed (fonte oficial), com o cabeça de chave primeiro.
                Selecoes = g.Selecoes
                    .OrderByDescending(s => s.CabecaDeChave)
                    .ThenBy(s => s.Id)
                    .Select(s => new SelecaoGrupoDto(s.Nome, s.CodigoFifa, s.CabecaDeChave))
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        return grupos
            .Select(g => new GrupoResumoDto(g.Codigo, g.Selecoes))
            .ToList();
    }
}
