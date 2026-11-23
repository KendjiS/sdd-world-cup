namespace PortalCopa26.Models;

/// <summary>Posição em campo de um jogador.</summary>
public enum Posicao
{
    Goleiro,
    Defensor,
    MeioCampista,
    Atacante
}

/// <summary>Fase da competição em que um jogo é disputado.</summary>
public enum Fase
{
    Grupos,
    SegundaFase,
    Oitavas,
    Quartas,
    Semifinal,
    TerceiroLugar,
    Final
}

/// <summary>País-sede da Copa do Mundo 2026.</summary>
public enum PaisSede
{
    Canada,
    EstadosUnidos,
    Mexico
}
