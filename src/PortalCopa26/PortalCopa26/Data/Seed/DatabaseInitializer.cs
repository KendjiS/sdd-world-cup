using Microsoft.EntityFrameworkCore;

namespace PortalCopa26.Data.Seed;

/// <summary>
/// Prepara o banco na inicialização: aplica as migrations pendentes e executa a
/// carga inicial dos dados oficiais.
/// </summary>
public class DatabaseInitializer(
    IDbContextFactory<PortalCopaDbContext> contextFactory,
    DatabaseSeeder seeder,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        logger.LogInformation("Banco de dados: {DataSource}", context.Database.GetDbConnection().DataSource);

        await context.Database.MigrateAsync(cancellationToken);
        await seeder.SeedAsync(context, cancellationToken);
    }
}
