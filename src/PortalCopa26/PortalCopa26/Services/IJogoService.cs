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

    /// <summary>
    /// Todos os jogos, ou apenas os do grupo <paramref name="grupo"/> quando informado,
    /// em ordem de data e hora e agrupados pela data em Brasília.
    /// </summary>
    Task<IReadOnlyList<DiaJogosDto>> ObterJogosAsync(
        string? grupo = null,
        CancellationToken cancellationToken = default);
}
