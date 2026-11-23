namespace PortalCopa26.Models;

/// <summary>Seleção participante da Copa do Mundo 2026.</summary>
public class Selecao
{
    private const string BandeiraUrlBase = "https://api.fifa.com/api/v3/picture/flags-sq-4/";

    public int Id { get; set; }

    /// <summary>Código FIFA de três letras (ex.: BRA).</summary>
    public string CodigoFifa { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Tecnico { get; set; } = string.Empty;

    public bool CabecaDeChave { get; set; }

    public int GrupoId { get; set; }

    public Grupo? Grupo { get; set; }

    public List<Jogador> Jogadores { get; set; } = [];

    /// <summary>URL da bandeira no padrão da API pública da FIFA (não persistida).</summary>
    public string BandeiraUrl => BandeiraUrlBase + CodigoFifa;
}
