namespace PortalCopa26.Models.Dtos;

/// <summary>Totais do torneio calculados a partir do banco.</summary>
public sealed record ResumoTorneioDto(int Selecoes, int Grupos, int Jogos, int Estadios);

/// <summary>País-sede com a quantidade de estádios registrados.</summary>
public sealed record PaisSedeDto(string Nome, string CodigoFifa, int QuantidadeEstadios);
