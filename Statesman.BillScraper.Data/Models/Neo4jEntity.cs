namespace Statesman.BillScraper.Data.Models;

public abstract class Neo4jEntity
{
    // Domain ID
    public int Id { get; set; }

    // Neo4j node ID
    public long NodeId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
