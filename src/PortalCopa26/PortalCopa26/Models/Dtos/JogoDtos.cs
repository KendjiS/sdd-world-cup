namespace PortalCopa26.Models.Dtos;

/// <summary>
/// Lado (mandante ou visitante) de um jogo. Quando a seleção ainda não está definida
/// (mata-mata), <see cref="CodigoFifa"/> é nulo e <see cref="Nome"/> traz a descrição da vaga.
/// </summary>
public sealed record LadoJogoDto(string Nome, string? CodigoFifa)
{
    public bool Definido => CodigoFifa is not null;
}

/// <summary>Resumo de um jogo para listagens, com horário já convertido para Brasília.</summary>
public sealed record JogoResumoDto(
    int Numero,
    Fase Fase,
    string? Rotulo,
    string? Grupo,
    DateTime DataHoraBrasilia,
    string Estadio,
    string Cidade,
    LadoJogoDto Mandante,
    LadoJogoDto Visitante);

/// <summary>Jogos de um mesmo dia (data no horário de Brasília), em ordem de horário.</summary>
public sealed record DiaJogosDto(DateOnly Data, IReadOnlyList<JogoResumoDto> Jogos);
