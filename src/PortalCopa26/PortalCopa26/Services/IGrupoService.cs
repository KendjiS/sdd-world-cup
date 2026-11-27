using PortalCopa26.Models.Dtos;

namespace PortalCopa26.Services;

/// <summary>Consultas dos grupos da fase de grupos.</summary>
public interface IGrupoService
{
    /// <summary>Grupos em ordem alfabética (A a L), cada um com as suas seleções.</summary>
    Task<IReadOnlyList<GrupoResumoDto>> ObterGruposAsync(CancellationToken cancellationToken = default);
}
