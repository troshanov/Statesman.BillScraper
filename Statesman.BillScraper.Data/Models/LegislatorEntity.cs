namespace Statesman.BillScraper.Data.Models;

public class LegislatorEntity
{
    // Domain ID
    public int Id { get; set; }

    // Neo4j node ID
    public long NodeId { get; set; }

    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    // Navigation property
    public ICollection<BillEntity> SponsoredBills { get; set; } = new List<BillEntity>();
}
