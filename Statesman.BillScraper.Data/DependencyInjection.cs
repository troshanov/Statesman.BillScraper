using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;
using Statesman.BillScraper.Data.Migration;
using Statesman.BillScraper.Data.Repositories;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data;

public static class DependencyInjection
{
    public static async Task AddDataLayer(this IServiceCollection services, IConfiguration configuration, bool isWorkerService = false)
    {
        var neo4jConfig = configuration.GetSection("Neo4j");

        var driver = GraphDatabase.Driver(
            neo4jConfig["Uri"]!,
            AuthTokens.Basic(neo4jConfig["Username"]!, neo4jConfig["Password"]!)
        );

        // Verify connectivity
        await driver.VerifyConnectivityAsync();

        services.AddSingleton(driver);
        services.AddSingleton<IMigrationRunner, Neo4jMigrationRunner>();

        if (isWorkerService)
        {    
            services.AddSingleton<IBillRepository, Neo4jBillRepository>();
            services.AddSingleton<ILegislatorRepository, Neo4jLegislatorRepository>();
        }
        else
        {
            services.AddScoped<IBillRepository, Neo4jBillRepository>();
            services.AddScoped<ILegislatorRepository, Neo4jLegislatorRepository>();
        }
    }

    public static async Task RunNeo4jMigrations(this IHost host)
    {
        try
        {
            var runner = host.Services.GetRequiredService<IMigrationRunner>();
            await runner.RunMigrationsAsync();

            Console.WriteLine("Migrations completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running migrations: {ex.Message}");
        }
    }
}
