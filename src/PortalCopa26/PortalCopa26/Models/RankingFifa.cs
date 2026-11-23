namespace PortalCopa26.Models;

/// <summary>
/// Entrada do ranking FIFA. Pode referir-se a seleções não classificadas para a Copa,
/// caso em que <see cref="SelecaoId"/> é nulo.
/// </summary>
public class RankingFifa
{
    public int Id { get; set; }

    public int Posicao { get; set; }

    public string NomeSelecao { get; set; } = string.Empty;

    public decimal Pontos { get; set; }

    public int? SelecaoId { get; set; }

    public Selecao? Selecao { get; set; }
}
