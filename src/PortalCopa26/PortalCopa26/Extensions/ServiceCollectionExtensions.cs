using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PortalCopa26.Data;
using PortalCopa26.Data.Seed;
using PortalCopa26.Services;

namespace PortalCopa26.Extensions;

public static class ServiceCollectionExtensions
{
    public const string ConnectionStringName = "PortalCopa";

    /// <summary>
    /// Registra a persistência (EF Core + SQLite). Um caminho relativo no Data Source é
    /// resolvido contra a raiz da aplicação (ContentRootPath), e não contra o diretório
    /// de execução, para que o mesmo banco seja usado independentemente de onde a
    /// aplicação for iniciada.
    /// </summary>
    public static IServiceCollection AddPortalCopaData(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"A connection string '{ConnectionStringName}' não está configurada.");

        var resolvedConnectionString = ResolveSqliteConnectionString(connectionString, environment.ContentRootPath);

        services.AddDbContextFactory<PortalCopaDbContext>(options =>
            options.UseSqlite(resolvedConnectionString));

        return services;
    }

    /// <summary>Registra os serviços de aplicação do portal.</summary>
    public static IServiceCollection AddPortalCopaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<DatabaseSeeder>();
        services.AddTransient<DatabaseInitializer>();

        services.TryAddSingleton(TimeProvider.System);
        services.Configure<ProximosJogosOptions>(configuration.GetSection(ProximosJogosOptions.SectionName));

        services.AddScoped<ITorneioService, TorneioService>();
        services.AddScoped<IJogoService, JogoService>();
        services.AddScoped<IGrupoService, GrupoService>();
        services.AddScoped<IRankingFifaService, RankingFifaService>();

        return services;
    }

    internal static string ResolveSqliteConnectionString(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.IsNullOrWhiteSpace(builder.DataSource)
            && builder.DataSource != ":memory:"
            && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, builder.DataSource));
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}
