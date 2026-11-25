using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

/// <summary>Consultas de jogos.</summary>
public interface IJogoService
{
    /// <summary>
    /// Jogos dos <paramref name="quantidadeDias"/> primeiros dias com partidas a partir de
    /// <paramref name="dataReferencia"/> (inclusive), agrupados pela data em Brasília.
    /// </summary>
    Task<IReadOnlyList<DiaJogosDto>> ObterProximosJogosAsync(
        DateOnly dataReferencia,
        int quantidadeDias,
        CancellationToken cancellationToken = default);
}
