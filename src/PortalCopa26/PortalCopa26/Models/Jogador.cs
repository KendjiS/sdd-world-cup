namespace PortalCopa26.Models;

/// <summary>Jogador convocado por uma seleção.</summary>
public class Jogador
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public Posicao Posicao { get; set; }

    public int Idade { get; set; }

    /// <summary>Gols marcados pela seleção.</summary>
    public int Gols { get; set; }

    /// <summary>Número de Copas do Mundo disputadas (0 quando não informado).</summary>
    public int ParticipacoesCopas { get; set; }

    public int SelecaoId { get; set; }

    public Selecao? Selecao { get; set; }
}
