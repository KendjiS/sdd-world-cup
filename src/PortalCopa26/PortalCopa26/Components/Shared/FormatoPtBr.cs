using System.Globalization;

namespace PortalCopa26.Components.Shared;

/// <summary>Formatações de data, hora e números em português do Brasil usadas pelos componentes.</summary>
public static class FormatoPtBr
{
    public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Ex.: "Quinta-feira, 11 de junho de 2026".</summary>
    public static string DiaPorExtenso(DateOnly data)
    {
        var texto = data.ToString("dddd, d 'de' MMMM 'de' yyyy", Cultura);
        return char.ToUpper(texto[0], Cultura) + texto[1..];
    }

    /// <summary>Ex.: "11 de junho".</summary>
    public static string DiaMes(DateOnly data) => data.ToString("d 'de' MMMM", Cultura);

    /// <summary>Ex.: "16:00".</summary>
    public static string Hora(DateTime dataHora) => dataHora.ToString("HH:mm", Cultura);

    /// <summary>Ex.: "87.523".</summary>
    public static string Inteiro(int valor) => valor.ToString("N0", Cultura);
}
