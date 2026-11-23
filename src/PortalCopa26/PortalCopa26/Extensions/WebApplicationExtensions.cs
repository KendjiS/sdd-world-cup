using PortalCopa26.Data.Seed;

namespace PortalCopa26.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>Cria ou atualiza o banco de dados antes de a aplicação começar a atender.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
    }
}
