namespace PortalCopa26.Models;

/// <summary>Simulação de resultados criada pelo usuário.</summary>
public class Simulacao
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateTime CriadaEmUtc { get; set; }

    public DateTime AtualizadaEmUtc { get; set; }

    public List<SimulacaoJogo> Jogos { get; set; } = [];
}
