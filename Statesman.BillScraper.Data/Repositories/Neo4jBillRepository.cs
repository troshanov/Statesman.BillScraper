using Neo4jClient;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data.Repositories;

public class Neo4jBillRepository : IBillRepository
{
    private readonly IGraphClient _client;

    public Neo4jBillRepository(IGraphClient client)
    {
        _client = client;
    }

    public async Task<Bill?> CreateBillAsync(Bill bill)
    {
        try
        {
            var query = _client.Cypher
                .Create("(b:Bill)")
                .Set("b = $bill")
                .WithParam("bill", new
                {
                    bill.Id,
                    bill.Title,
                    bill.Sign,
                    bill.RawText,
                    bill.Date,
                    bill.IsParsed,
                    bill.ParsedAt,
                    bill.UpdatedAt
                })
                .Return((b) => new
                {
                    Bill = b.As<Bill>(),
                    NodeId = b.Id()
                });

            var results = await query.ResultsAsync;
            var result = results.FirstOrDefault();

            if (result != null)
            {
                result.Bill.NodeId = result.NodeId;
                return result.Bill;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bill: {ex.Message}");

            return null;
        }
    }

    public async Task<Bill?> GetBillByIdAsync(int id)
    {
        var query = _client.Cypher
            .Match("(b:Bill)")
            .Where((Bill b) => b.Id == id)
            .Return((b) => new
            {
                Bill = b.As<Bill>(),
                NodeId = b.Id()
            });

        var results = await query.ResultsAsync;
        var result = results.FirstOrDefault();

        if (result != null)
        {
            result.Bill.NodeId = result.NodeId;
            return result.Bill;
        }

        return null;
    }

    public async Task<Bill?> GetBillByNodeIdAsync(string nodeId)
    {
        var query = _client.Cypher
            .Match("(b:Bill)")
            .Where("ID(b) = $nodeId")
            .WithParam("nodeId", nodeId)
            .Return((b) => new
            {
                Bill = b.As<Bill>(),
                NodeId = b.Id()
            });

        var results = await query.ResultsAsync;
        var result = results.FirstOrDefault();

        if (result != null)
        {
            result.Bill.NodeId = result.NodeId;
            return result.Bill;
        }

        return null;
    }

    public async Task AddSponsorToBillAsync(int billId, int legislatorId)
    {
        await _client.Cypher
            .Match("(b:Bill)", "(l:Legislator)")
            .Where((Bill b) => b.Id == billId)
            .AndWhere((Legislator l) => l.Id == legislatorId)
            .Create("(l)-[:SPONSORS]->(b)")
            .ExecuteWithoutResultsAsync();
    }

    public async Task<IEnumerable<Legislator>> GetBillSponsorsAsync(int billId)
    {
        var query = _client.Cypher
            .Match("(l:Legislator)-[:SPONSORS]->(b:Bill)")
            .Where((Bill b) => b.Id == billId)
            .Return((l) => new
            {
                Legislator = l.As<Legislator>(),
                NodeId = l.Id()
            });

        var results = await query.ResultsAsync;

        return results.Select(r =>
        {
            r.Legislator.NodeId = r.NodeId;
            return r.Legislator;
        });
    }

    public async Task<IEnumerable<Bill>> GetUnparsedBillsAsync()
    {
        var query = _client.Cypher
            .Match("(b:Bill)")
            .Where((Bill b) => b.IsParsed == false)
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
