using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Respawn;
using Respawn.Graph;
using RecipeVault.Infrastructure.Persistence;

namespace RecipeVault.IntegrationTests;

/// <summary>
/// Hosts the real API in memory against a real SQL Server database.
/// Locally that's LocalDB; in CI it's the SQL Server service container.
/// Set RECIPEVAULT_TEST_DB to use a different server (e.g. an Azure SQL test database).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DefaultConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=RecipeVault_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    private const string TestJwtSecret = "integration-tests-only-secret-0123456789abcdef";

    private readonly string _connectionString;
    private Respawner? _respawner;

    public ApiFactory()
    {
        _connectionString = Environment.GetEnvironmentVariable("RECIPEVAULT_TEST_DB") ?? DefaultConnectionString;

        // Respawn deletes every row between tests, so refuse to point it at anything that isn't a test database.
        if (!_connectionString.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The integration test database name must contain 'test'.");
        }

        // Program.cs reads configuration before the host is built, so pass settings as environment variables.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _connectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", TestJwtSecret);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");

    public async Task InitializeAsync()
    {
        await DatabaseInitializer.MigrateAsync(Services, seed: false);

        _respawner = await Respawner.CreateAsync(_connectionString, new RespawnerOptions
        {
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
        });
    }

    /// <summary>Empties every table so each test starts from a clean database.</summary>
    public Task ResetDatabaseAsync() =>
        _respawner?.ResetAsync(_connectionString)
        ?? throw new InvalidOperationException("InitializeAsync has not run.");

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
