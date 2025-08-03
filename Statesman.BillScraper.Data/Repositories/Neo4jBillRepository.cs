using Neo4j.Driver;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data.Repositories;

public class Neo4jBillRepository : IBillRepository
{
    private readonly IDriver _driver;

    public Neo4jBillRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<Bill?> CreateBillAsync(Bill bill)
    {
        try
        {
            await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var query = @"
                    CREATE (b:Bill {
                        Id: $id,
                        Title: $title,
                        Sign: $sign,
                        RawText: $rawText,
                        Date: $date,
                        IsParsed: $isParsed,
                        ParsedAt: $parsedAt,
                        UpdatedAt: $updatedAt
                    })
                    RETURN b, ID(b) as nodeId";

                var cursor = await tx.RunAsync(query, new
                {
                    id = bill.Id,
                    title = bill.Title,
                    sign = bill.Sign,
                    rawText = bill.RawText,
                    date = bill.Date,
                    isParsed = bill.IsParsed,
                    parsedAt = bill.ParsedAt,
                    updatedAt = bill.UpdatedAt
                });

                var record = await cursor.SingleAsync();
                var node = record["b"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return MapNodeToBill(node, nodeId);
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bill: {ex.Message}");
            return null;
        }
    }

    public async Task<Bill?> GetBillByIdAsync(int id)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {Id: $id})
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query, new { id });
            var records = await cursor.ToListAsync();

            if (!records.Any())
                return null;

            var record = records.First();
            var node = record["b"].As<INode>();
            var nodeId = record["nodeId"].As<long>();

            return MapNodeToBill(node, nodeId);
        });
    }

    public async Task<Bill?> GetBillByNodeIdAsync(long nodeId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill)
                WHERE ID(b) = $nodeId
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query, new { nodeId });
            var records = await cursor.ToListAsync();

            if (!records.Any())
                return null;

            var record = records.First();
            var node = record["b"].As<INode>();

            return MapNodeToBill(node, nodeId);
        });
    }

    public async Task AddSponsorToBillAsync(int billId, int legislatorId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {Id: $billId}), (l:Legislator {Id: $legislatorId})
                CREATE (l)-[:SPONSORS]->(b)";

            await tx.RunAsync(query, new { billId, legislatorId });
        });
    }

    public async Task<IEnumerable<Legislator>> GetBillSponsorsAsync(int billId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (l:Legislator)-[:SPONSORS]->(b:Bill {Id: $billId})
                RETURN l, ID(l) as nodeId";

            var cursor = await tx.RunAsync(query, new { billId });
            var records = await cursor.ToListAsync();

            return records.Select(record =>
            {
                var node = record["l"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return new Legislator
                {
                    Id = node.Properties["Id"].As<int>(),
                    FirstName = node.Properties["FirstName"].As<string>(),
                    MiddleName = node.Properties["MiddleName"].As<string>(),
                    LastName = node.Properties["LastName"].As<string>(),
                    NodeId = nodeId
                };
            }).ToList();
        });
    }

    public async Task<IEnumerable<Bill>> GetUnparsedBillsAsync()
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase("statesman"));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {IsParsed: false})
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query);
            var records = await cursor.ToListAsync();

            return records.Select(record =>
            {
                var node = record["b"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return MapNodeToBill(node, nodeId);
            }).ToList();
        });
    }

    private Bill MapNodeToBill(INode node, long nodeId)
    {
        return new Bill
        {
            Id = node.Properties["Id"].As<int>(),
            Title = node.Properties["Title"].As<string>(),
            Sign = node.Properties["Sign"].As<string>(),
            RawText = node.Properties["RawText"].As<string>(),
            Date = node.Properties["Date"].As<DateTime>(),
            IsParsed = node.Properties["IsParsed"].As<bool>(),
            ParsedAt = node.Properties.ContainsKey("ParsedAt") ? node.Properties["ParsedAt"].As<DateTime?>() : null,
            UpdatedAt = node.Properties["UpdatedAt"].As<DateTime>(),
            NodeId = nodeId
        };
    }
}
