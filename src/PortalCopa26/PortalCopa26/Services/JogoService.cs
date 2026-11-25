using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

public sealed class JogoService(IDbContextFactory<PortalCopaDbContext> dbFactory) : IJogoService
{
    public async Task<IReadOnlyList<DiaJogosDto>> ObterProximosJogosAsync(
        DateOnly dataReferencia,
        int quantidadeDias,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantidadeDias);

        var limiteUtc = HorarioBrasilia.InicioDoDiaUtc(dataReferencia);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // No máximo 104 jogos: buscar os futuros e agrupar por dia em memória.
        var jogos = await db.Jogos
            .AsNoTracking()
            .Where(j => j.DataHoraUtc >= limiteUtc)
            .OrderBy(j => j.DataHoraUtc)
            .ThenBy(j => j.Numero)
            .Select(j => new
            {
                j.Numero,
                j.Fase,
                j.Rotulo,
                j.DataHoraUtc,
                Grupo = j.Grupo != null ? j.Grupo.Codigo : null,
                Estadio = j.Estadio!.Nome,
                Cidade = j.Estadio.Cidade,
                MandanteNome = j.SelecaoMandante != null ? j.SelecaoMandante.Nome : null,
                MandanteCodigo = j.SelecaoMandante != null ? j.SelecaoMandante.CodigoFifa : null,
                j.DescricaoMandante,
                VisitanteNome = j.SelecaoVisitante != null ? j.SelecaoVisitante.Nome : null,
                VisitanteCodigo = j.SelecaoVisitante != null ? j.SelecaoVisitante.CodigoFifa : null,
                j.DescricaoVisitante,
            })
            .ToListAsync(cancellationToken);

        return jogos
            .Select(j => new JogoResumoDto(
                j.Numero,
                j.Fase,
                j.Rotulo,
                j.Grupo,
                HorarioBrasilia.ParaBrasilia(j.DataHoraUtc),
                j.Estadio,
                j.Cidade,
                Lado(j.MandanteNome, j.MandanteCodigo, j.DescricaoMandante),
                Lado(j.VisitanteNome, j.VisitanteCodigo, j.DescricaoVisitante)))
            .GroupBy(j => DateOnly.FromDateTime(j.DataHoraBrasilia))
            .Take(quantidadeDias)
            .Select(g => new DiaJogosDto(g.Key, g.ToList()))
            .ToList();
    }

    private static LadoJogoDto Lado(string? nome, string? codigo, string? descricao) =>
        nome is not null && codigo is not null
            ? new LadoJogoDto(nome, codigo)
            : new LadoJogoDto(descricao ?? "A definir", null);
}
