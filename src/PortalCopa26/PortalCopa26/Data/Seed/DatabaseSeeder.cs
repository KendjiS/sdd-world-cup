using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PortalCopa26.Models;

namespace PortalCopa26.Data.Seed;

/// <summary>
/// Carga inicial idempotente dos dados oficiais da Copa 2026 a partir de
/// <c>Data/Seed/*.json</c> (gerados por <c>tools/seed-generator</c>).
/// Cada tabela só é carregada se estiver vazia. Simulações nunca são tocadas.
/// </summary>
public class DatabaseSeeder(ILogger<DatabaseSeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _seedDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "Seed");

    public async Task SeedAsync(PortalCopaDbContext context, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var carregadas = 0;
        carregadas += await SeedTableAsync(context.Grupos, "grupos.json", SeedGruposAsync, context, cancellationToken);
        carregadas += await SeedTableAsync(context.Selecoes, "selecoes.json", SeedSelecoesAsync, context, cancellationToken);
        carregadas += await SeedTableAsync(context.Estadios, "estadios.json", SeedEstadiosAsync, context, cancellationToken);
        carregadas += await SeedTableAsync(context.Jogos, "jogos.json", SeedJogosAsync, context, cancellationToken);
        carregadas += await SeedTableAsync(context.Jogadores, "jogadores.json", SeedJogadoresAsync, context, cancellationToken);
        carregadas += await SeedTableAsync(context.RankingFifa, "ranking.json", SeedRankingAsync, context, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        if (carregadas == 0)
        {
            logger.LogInformation("Seed já aplicado: todas as tabelas de dados oficiais já estavam populadas.");
        }
    }

    private async Task<int> SeedTableAsync<TEntity>(
        DbSet<TEntity> set,
        string arquivo,
        Func<PortalCopaDbContext, string, CancellationToken, Task<int>> seed,
        PortalCopaDbContext context,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var tabela = context.Model.FindEntityType(typeof(TEntity))?.GetTableName() ?? typeof(TEntity).Name;

        if (await set.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed {Tabela}: já populada, ignorada.", tabela);
            return 0;
        }

        var total = await seed(context, arquivo, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();

        logger.LogInformation("Seed {Tabela}: {Total} registros carregados.", tabela, total);
        return 1;
    }

    private async Task<int> SeedGruposAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<GrupoSeed>(arquivo, ct);
        context.Grupos.AddRange(dados.Select(d => new Grupo { Codigo = d.Codigo }));
        return dados.Count;
    }

    private async Task<int> SeedSelecoesAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<SelecaoSeed>(arquivo, ct);
        var grupos = await context.Grupos.ToDictionaryAsync(g => g.Codigo, g => g.Id, ct);

        context.Selecoes.AddRange(dados.Select(d => new Selecao
        {
            CodigoFifa = d.CodigoFifa,
            Nome = d.Nome,
            Tecnico = d.Tecnico,
            CabecaDeChave = d.CabecaDeChave,
            GrupoId = Resolver(grupos, d.Grupo, "grupo")
        }));
        return dados.Count;
    }

    private async Task<int> SeedEstadiosAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<EstadioSeed>(arquivo, ct);
        context.Estadios.AddRange(dados.Select(d => new Estadio
        {
            Nome = d.Nome,
            Cidade = d.Cidade,
            Pais = d.Pais,
            Capacidade = d.Capacidade
        }));
        return dados.Count;
    }

    private async Task<int> SeedJogosAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<JogoSeed>(arquivo, ct);
        var grupos = await context.Grupos.ToDictionaryAsync(g => g.Codigo, g => g.Id, ct);
        var selecoes = await context.Selecoes.ToDictionaryAsync(s => s.CodigoFifa, s => s.Id, ct);
        var estadios = await context.Estadios.ToDictionaryAsync(e => e.Cidade, e => e.Id, ct);

        context.Jogos.AddRange(dados.Select(d => new Jogo
        {
            Numero = d.Numero,
            Fase = d.Fase,
            Rotulo = d.Rotulo,
            DataHoraUtc = d.DataHoraUtc.UtcDateTime,
            GrupoId = d.Grupo is null ? null : Resolver(grupos, d.Grupo, "grupo"),
            EstadioId = Resolver(estadios, d.CidadeEstadio, "estádio"),
            SelecaoMandanteId = d.Mandante is null ? null : Resolver(selecoes, d.Mandante, "seleção"),
            SelecaoVisitanteId = d.Visitante is null ? null : Resolver(selecoes, d.Visitante, "seleção"),
            DescricaoMandante = d.DescricaoMandante,
            DescricaoVisitante = d.DescricaoVisitante
        }));
        return dados.Count;
    }

    private async Task<int> SeedJogadoresAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<JogadorSeed>(arquivo, ct);
        var selecoes = await context.Selecoes.ToDictionaryAsync(s => s.CodigoFifa, s => s.Id, ct);

        context.Jogadores.AddRange(dados.Select(d => new Jogador
        {
            Nome = d.Nome,
            Posicao = d.Posicao,
            Idade = d.Idade,
            Gols = d.Gols,
            ParticipacoesCopas = d.ParticipacoesCopas,
            SelecaoId = Resolver(selecoes, d.Selecao, "seleção")
        }));
        return dados.Count;
    }

    private async Task<int> SeedRankingAsync(PortalCopaDbContext context, string arquivo, CancellationToken ct)
    {
        var dados = await LerAsync<RankingSeed>(arquivo, ct);
        var selecoes = await context.Selecoes.ToDictionaryAsync(s => s.CodigoFifa, s => s.Id, ct);

        context.RankingFifa.AddRange(dados.Select(d => new RankingFifa
        {
            Posicao = d.Posicao,
            NomeSelecao = d.NomeSelecao,
            Pontos = d.Pontos,
            SelecaoId = d.Selecao is null ? null : Resolver(selecoes, d.Selecao, "seleção")
        }));
        return dados.Count;
    }

    private async Task<List<T>> LerAsync<T>(string arquivo, CancellationToken ct)
    {
        var caminho = Path.Combine(_seedDirectory, arquivo);
        await using var stream = File.OpenRead(caminho);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException($"Arquivo de seed vazio ou inválido: {caminho}");
    }

    private static int Resolver(Dictionary<string, int> mapa, string chave, string tipo) =>
        mapa.TryGetValue(chave, out var id)
            ? id
            : throw new InvalidOperationException($"Seed: {tipo} '{chave}' não encontrado.");

    private sealed record GrupoSeed(string Codigo);

    private sealed record SelecaoSeed(string CodigoFifa, string Nome, string Tecnico, string Grupo, bool CabecaDeChave);

    private sealed record EstadioSeed(string Nome, string Cidade, PaisSede Pais, int Capacidade);

    private sealed record JogoSeed(
        int Numero,
        Fase Fase,
        string? Rotulo,
        string? Grupo,
        string CidadeEstadio,
        DateTimeOffset DataHoraUtc,
        string? Mandante,
        string? Visitante,
        string? DescricaoMandante,
        string? DescricaoVisitante);

    private sealed record JogadorSeed(
        string Selecao,
        string Nome,
        Posicao Posicao,
        int Idade,
        int Gols,
        int ParticipacoesCopas);

    private sealed record RankingSeed(int Posicao, string NomeSelecao, decimal Pontos, string? Selecao);
}
