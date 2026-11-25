using System.Globalization;

namespace PortalCopa26.Services;

/// <summary>Configuração da seção "Próximos Jogos" (<c>PortalCopa:ProximosJogos</c>).</summary>
public sealed class ProximosJogosOptions
{
    public const string SectionName = "PortalCopa:ProximosJogos";

    /// <summary>
    /// Data de referência no formato <c>yyyy-MM-dd</c>, interpretada no horário de Brasília.
    /// É texto, e não <see cref="DateOnly"/>, para que um valor inválido caia na data atual
    /// em vez de lançar exceção na leitura da configuração.
    /// </summary>
    public string? DataReferencia { get; set; }

    /// <summary>Data configurada ou, se ausente ou inválida, a data atual em Brasília.</summary>
    public DateOnly ObterDataReferencia(TimeProvider timeProvider) =>
        DateOnly.TryParseExact(DataReferencia, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var data)
            ? data
            : HorarioBrasilia.Hoje(timeProvider);
}
