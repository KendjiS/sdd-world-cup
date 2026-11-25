using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

/// <summary>Informações gerais do torneio.</summary>
public interface ITorneioService
{
    /// <summary>Totais de seleções, grupos, jogos e estádios registrados no banco.</summary>
    Task<ResumoTorneioDto> ObterResumoAsync(CancellationToken cancellationToken = default);

    /// <summary>Países-sede (Estados Unidos, Canadá, México) com a quantidade de estádios de cada um.</summary>
    Task<IReadOnlyList<PaisSedeDto>> ObterPaisesSedeAsync(CancellationToken cancellationToken = default);
}
