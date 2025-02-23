using Neo4jClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Statesman.BillScraper.Data.Migration;
using Statesman.BillScraper.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data;

public static class DependencyInjection
{
    public static async Task<GraphClient> AddDataLayer(this IServiceCollection services, IConfiguration configuration, bool isWorkerService = false)
    {
        var neo4jConfig = configuration.GetSection("Neo4j");

        var client = new GraphClient(
            new Uri(neo4jConfig["Uri"]!),
            neo4jConfig["Username"],
            neo4jConfig["Password"]
        );

        await client.ConnectAsync();

        services.AddSingleton<IGraphClient>(client);
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
            
        return client;
    }

    public static async Task RunNeo4jMigrations(this IHost host, GraphClient client)
    {
        try
        {
            if (!client.IsConnected)
                await client.ConnectAsync();

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
