using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

/// <summary>Consultas do ranking FIFA.</summary>
public interface IRankingFifaService
{
    /// <summary>As <paramref name="quantidade"/> primeiras posições do ranking, em ordem de posição.</summary>
    Task<IReadOnlyList<RankingItemDto>> ObterTopAsync(int quantidade, CancellationToken cancellationToken = default);
}
