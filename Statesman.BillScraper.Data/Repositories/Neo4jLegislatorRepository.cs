using Neo4jClient;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data.Repositories;

public class Neo4jLegislatorRepository : ILegislatorRepository
{
    private readonly IGraphClient _client;

    public Neo4jLegislatorRepository(IGraphClient client)
    {
        _client = client;
    }

    public async Task<Legislator?> CreateLegislatorAsync(Legislator legislator)
    {
        try
        {
            var query = _client.Cypher
                .Create("(l:Legislator)")
                .Set("l = $legislator")
                .WithParam("legislator", new
                {
                    legislator.Id,
                    legislator.FirstName,
                    legislator.MiddleName,
                    legislator.LastName
                })
                .Return((l) => new
                {
                    Legislator = l.As<Legislator>(),
                    NodeId = l.Id()
                });

            var results = await query.ResultsAsync;
            var result = results.FirstOrDefault();

            if (result != null)
            {
                result.Legislator.NodeId = result.NodeId;
                return result.Legislator;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating legislator: {ex.Message}");

            return null;
        }
    }

    public async Task<Legislator?> GetLegislatorByIdAsync(int id)
    {
        var query = _client.Cypher
            .Match("(l:Legislator)")
            .Where((Legislator l) => l.Id == id)
            .Return((l) => new
            {
                Legislator = l.As<Legislator>(),
                NodeId = l.Id()
            });

        var results = await query.ResultsAsync;
        var result = results.FirstOrDefault();

        if (result != null)
        {
            result.Legislator.NodeId = result.NodeId;
            return result.Legislator;
        }
        return null;
    }

    public async Task<Legislator?> GetLegislatorByNodeIdAsync(string nodeId)
    {
        var query = _client.Cypher
            .Match("(l:Legislator)")
            .Where("ID(l) = $nodeId")
            .WithParam("nodeId", nodeId)
            .Return((l) => new
            {
                Legislator = l.As<Legislator>(),
                NodeId = l.Id()
            });

        var results = await query.ResultsAsync;
        var result = results.FirstOrDefault();

        if (result != null)
        {
            result.Legislator.NodeId = result.NodeId;
            return result.Legislator;
        }
        return null;
    }

    public async Task<IEnumerable<Bill>> GetSponsoredBillsAsync(int legislatorId)
    {
        var query = _client.Cypher
            .Match("(l:Legislator)-[:SPONSORS]->(b:Bill)")
            .Where((Legislator l) => l.Id == legislatorId)
            .Return((b) => new
            {
                Bill = b.As<Bill>(),
                NodeId = b.Id()
            });

        var results = await query.ResultsAsync;
        return results.Select(r =>
        {
            r.Bill.NodeId = r.NodeId;
            return r.Bill;
        });
    }
}
