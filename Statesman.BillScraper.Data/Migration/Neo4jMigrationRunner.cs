using Neo4jClient;
using Microsoft.Extensions.Logging;

namespace Statesman.BillScraper.Data.Migration;

public class Neo4jMigrationRunner : IMigrationRunner
{
    private readonly IGraphClient _client;
    private readonly ILogger<Neo4jMigrationRunner> _logger;

    public Neo4jMigrationRunner(
        IGraphClient client,
        ILogger<Neo4jMigrationRunner> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task RunMigrationsAsync()
    {
        try
        {
            await EnsureMigrationTrackingExistsAsync();

            var migrations = Neo4jMigrations.GetMigrations();
            var appliedMigrations = await GetAppliedMigrationsAsync();
            foreach (var migration in migrations.OrderBy(m => m.Version))
            {
                if (!appliedMigrations.Contains(migration.Version))
                {
                    _logger.LogInformation("Applying migration {Version}: {Description}",
                        migration.Version, migration.Description);

                    await ApplyMigrationAsync(migration);
                    await RecordMigrationAsync(migration);

                    _logger.LogInformation("Successfully applied migration {Version}", migration.Version);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying migrations");
            throw;
        }
    }

    private async Task EnsureMigrationTrackingExistsAsync()
    {
        await _client.Cypher
            .Create(@"
                CONSTRAINT migration_version_unique IF NOT EXISTS 
                FOR (m:Migration) REQUIRE m.Version IS UNIQUE")
            .ExecuteWithoutResultsAsync();
    }

    private async Task<HashSet<string>> GetAppliedMigrationsAsync()
    {

        var result = await _client.Cypher
            .Match("(m:Migration)")
            .Return(m => m.As<Migration>().Version)
            .ResultsAsync;

        return result.ToHashSet();
    }

    private async Task ApplyMigrationAsync(Migration migration)
    {
        foreach (var statement in migration.UpStatements)
        {
            switch (statement.QueryType)
            {
                case Neo4jQueryType.Create:
                    await _client.Cypher
                        .Create(statement.Statement)
                        .ExecuteWithoutResultsAsync();
                    break;

                case Neo4jQueryType.Drop:
                    await _client.Cypher
                        .Drop(statement.Statement)
                        .ExecuteWithoutResultsAsync();
                    break;
                
                default:
                    break;
            }
        }
    }

    private async Task RecordMigrationAsync(Migration migration)
    {
        await _client.Cypher
            .Create("(m:Migration)")
            .Set("m = $migration")
            .WithParam("migration", new
            {
                migration.Version,
                migration.Description,
                AppliedAt = DateTime.UtcNow
            })
            .ExecuteWithoutResultsAsync();
    }
}
