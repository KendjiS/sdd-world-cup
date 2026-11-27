namespace PortalCopa26.Models;

/// <summary>Nome de exibição e código FIFA de cada país-sede.</summary>
public static class PaisSedeExtensions
{
    /// <summary>Países-sede na ordem de exibição do portal.</summary>
    public static readonly IReadOnlyList<PaisSede> OrdemExibicao =
        [PaisSede.EstadosUnidos, PaisSede.Canada, PaisSede.Mexico];

    public static string Nome(this PaisSede pais) => pais switch
    {
        PaisSede.EstadosUnidos => "Estados Unidos",
        PaisSede.Canada => "Canadá",
        PaisSede.Mexico => "México",
        _ => throw new ArgumentOutOfRangeException(nameof(pais), pais, null),
    };

    public static string CodigoFifa(this PaisSede pais) => pais switch
    {
        PaisSede.EstadosUnidos => "USA",
        PaisSede.Canada => "CAN",
        PaisSede.Mexico => "MEX",
        _ => throw new ArgumentOutOfRangeException(nameof(pais), pais, null),
    };
}
