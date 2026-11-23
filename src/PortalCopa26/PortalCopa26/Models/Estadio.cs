namespace PortalCopa26.Models;

/// <summary>Estádio de uma das cidades-sede.</summary>
public class Estadio
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public PaisSede Pais { get; set; }

    public int Capacidade { get; set; }

    public List<Jogo> Jogos { get; set; } = [];
}
