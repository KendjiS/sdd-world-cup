namespace PortalCopa26.Models.Dtos;

/// <summary>Entrada do ranking FIFA. <see cref="CodigoFifa"/> é nulo para seleções fora da Copa.</summary>
public sealed record RankingItemDto(int Posicao, string Selecao, string? CodigoFifa, decimal Pontos);
