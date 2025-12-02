using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data.Repositories;

public class Neo4jLegislatorRepository : ILegislatorRepository
{
    private readonly IDriver _driver;
    private readonly string _database;
    public Neo4jLegislatorRepository(IDriver driver, IConfiguration configuration)
    {
        _driver = driver;
        _database = configuration.GetSection("Neo4j")["Database"]!;
    }

    public async Task<LegislatorEntity?> CreateLegislatorAsync(LegislatorEntity legislator)
    {
        try
        {
            await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var query = @"
                    CREATE (l:Legislator {
                        Id: $id,
                        FirstName: $firstName,
                        MiddleName: $middleName,
                        LastName: $lastName
                    })
                    RETURN l, ID(l) as nodeId";

                var cursor = await tx.RunAsync(query, new
                {
                    id = legislator.Id,
                    firstName = legislator.FirstName,
                    middleName = legislator.MiddleName,
                    lastName = legislator.LastName
                });

                var record = await cursor.SingleAsync();
                var node = record["l"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return new LegislatorEntity
                {
                    Id = node.Properties["Id"].As<int>(),
                    FirstName = node.Properties["FirstName"].As<string>(),
                    MiddleName = node.Properties["MiddleName"].As<string>(),
                    LastName = node.Properties["LastName"].As<string>(),
                    NodeId = nodeId
                };
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating legislator: {ex.Message}");
            return null;
        }
    }

    public async Task<LegislatorEntity?> GetLegislatorByIdAsync(int id)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (l:Legislator {Id: $id})
                RETURN l, ID(l) as nodeId";

            var cursor = await tx.RunAsync(query, new { id });
            var records = await cursor.ToListAsync();

            if (!records.Any())
                return null;

            var record = records.First();
            var node = record["l"].As<INode>();
            var nodeId = record["nodeId"].As<long>();

            return new LegislatorEntity
            {
                Id = node.Properties["Id"].As<int>(),
                FirstName = node.Properties["FirstName"].As<string>(),
                MiddleName = node.Properties["MiddleName"].As<string>(),
                LastName = node.Properties["LastName"].As<string>(),
                NodeId = nodeId
            };
        });
    }

    public async Task<LegislatorEntity?> GetLegislatorByNodeIdAsync(long nodeId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (l:Legislator)
                WHERE ID(l) = $nodeId
                RETURN l, ID(l) as nodeId";

            var cursor = await tx.RunAsync(query, new { nodeId });
            var records = await cursor.ToListAsync();

            if (!records.Any())
                return null;

            var record = records.First();
            var node = record["l"].As<INode>();

            return new LegislatorEntity
            {
                Id = node.Properties["Id"].As<int>(),
                FirstName = node.Properties["FirstName"].As<string>(),
                MiddleName = node.Properties["MiddleName"].As<string>(),
                LastName = node.Properties["LastName"].As<string>(),
                NodeId = nodeId
            };
        });
    }

    public async Task<IEnumerable<BillEntity>> GetSponsoredBillsAsync(int legislatorId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (l:Legislator {Id: $legislatorId})-[:SPONSORS]->(b:Bill)
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query, new { legislatorId });
            var records = await cursor.ToListAsync();

            return records.Select(record =>
            {
                var node = record["b"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return new BillEntity
                {
                    Id = node.Properties["Id"].As<int>(),
                    Title = node.Properties["Title"].As<string>(),
                    Sign = node.Properties["Sign"].As<string>(),
                    PdfUrl = node.Properties["PdfUrl"].As<string>(),
                    Date = node.Properties["Date"].As<DateTime>(),
                    IsParsed = node.Properties["IsParsed"].As<bool>(),
                    ParsedAt = node.Properties.ContainsKey("ParsedAt") ? node.Properties["ParsedAt"].As<DateTime?>() : null,
                    UpdatedAt = node.Properties["UpdatedAt"].As<DateTime>(),
                    NodeId = nodeId
                };
            }).ToList();
        });
    }
}
