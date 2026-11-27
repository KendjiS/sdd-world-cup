namespace PortalCopa26.Models.Dtos;

/// <summary>Seleção de um grupo.</summary>
public sealed record SelecaoGrupoDto(string Nome, string CodigoFifa, bool CabecaDeChave);

/// <summary>Grupo da fase de grupos com as suas seleções (cabeça de chave primeiro).</summary>
public sealed record GrupoResumoDto(string Codigo, IReadOnlyList<SelecaoGrupoDto> Selecoes);
