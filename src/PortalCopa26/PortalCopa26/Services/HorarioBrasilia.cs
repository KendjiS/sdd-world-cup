namespace PortalCopa26.Services;

/// <summary>
/// Conversões entre UTC e o horário de Brasília (UTC−3 fixo, sem horário de verão desde 2019),
/// o mesmo offset usado pelo seed para gravar os horários oficiais em UTC.
/// </summary>
public static class HorarioBrasilia
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-3);

    /// <summary>
    /// Converte um horário UTC para Brasília. O valor é tratado como UTC qualquer que seja o
    /// <see cref="DateTime.Kind"/>: o SQLite devolve <see cref="DateTimeKind.Unspecified"/>, e
    /// <c>ToLocalTime</c>/<c>ToUniversalTime</c> dependeriam do fuso do servidor.
    /// </summary>
    public static DateTime ParaBrasilia(DateTime utc) =>
        DateTime.SpecifyKind(utc + Offset, DateTimeKind.Unspecified);

    /// <summary>Instante UTC correspondente à meia-noite de <paramref name="data"/> em Brasília.</summary>
    public static DateTime InicioDoDiaUtc(DateOnly data) =>
        DateTime.SpecifyKind(data.ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);

    /// <summary>Data atual no horário de Brasília.</summary>
    public static DateOnly Hoje(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(ParaBrasilia(timeProvider.GetUtcNow().UtcDateTime));
}
