using Neo4j.Driver;
using Microsoft.Extensions.Logging;

namespace Statesman.BillScraper.Data.Migration;

public class Neo4jMigrationRunner : IMigrationRunner
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jMigrationRunner> _logger;

    public Neo4jMigrationRunner(
        IDriver driver,
        ILogger<Neo4jMigrationRunner> logger)
    {
        _driver = driver;
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
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                CREATE CONSTRAINT migration_version_unique IF NOT EXISTS 
                FOR (m:Migration) REQUIRE m.Version IS UNIQUE";

            await tx.RunAsync(query);
        });
    }

    private async Task<HashSet<string>> GetAppliedMigrationsAsync()
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = "MATCH (m:Migration) RETURN m.Version as version";
            var cursor = await tx.RunAsync(query);
            var records = await cursor.ToListAsync();

            return records.Select(r => r["version"].As<string>()).ToHashSet();
        });
    }

    private async Task ApplyMigrationAsync(Migration migration)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (var statement in migration.UpStatements)
            {
                await tx.RunAsync(statement);
            }
        });
    }

    private async Task RecordMigrationAsync(Migration migration)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                CREATE (m:Migration {
                    Version: $version,
                    Description: $description,
                    AppliedAt: $appliedAt
                })";

            await tx.RunAsync(query, new
            {
                version = migration.Version,
                description = migration.Description,
                appliedAt = DateTime.UtcNow
            });
        });
    }
}
