namespace PortalCopa26.Models;

/// <summary>
/// Jogo oficial da Copa. Em jogos de mata-mata ainda não definidos, as seleções
/// ficam nulas e as vagas são descritas em <see cref="DescricaoMandante"/> e
/// <see cref="DescricaoVisitante"/> (ex.: "Venc. Segundafase 1").
/// </summary>
public class Jogo
{
    public int Id { get; set; }

    /// <summary>Número sequencial do jogo (1 a 104).</summary>
    public int Numero { get; set; }

    public Fase Fase { get; set; }

    /// <summary>Rótulo do jogo no chaveamento (ex.: "Segundafase 9", "Final").</summary>
    public string? Rotulo { get; set; }

    /// <summary>Data e hora do jogo em UTC.</summary>
    public DateTime DataHoraUtc { get; set; }

    public int? GrupoId { get; set; }

    public Grupo? Grupo { get; set; }

    public int EstadioId { get; set; }

    public Estadio? Estadio { get; set; }

    public int? SelecaoMandanteId { get; set; }

    public Selecao? SelecaoMandante { get; set; }

    public int? SelecaoVisitanteId { get; set; }

    public Selecao? SelecaoVisitante { get; set; }

    public string? DescricaoMandante { get; set; }

    public string? DescricaoVisitante { get; set; }

    public int? GolsMandante { get; set; }

    public int? GolsVisitante { get; set; }
}
