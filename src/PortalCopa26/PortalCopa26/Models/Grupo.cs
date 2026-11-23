namespace PortalCopa26.Models;

/// <summary>Grupo da fase de grupos (A a L).</summary>
public class Grupo
{
    public int Id { get; set; }

    /// <summary>Letra do grupo, de "A" a "L".</summary>
    public string Codigo { get; set; } = string.Empty;

    public List<Selecao> Selecoes { get; set; } = [];

    public List<Jogo> Jogos { get; set; } = [];
}
