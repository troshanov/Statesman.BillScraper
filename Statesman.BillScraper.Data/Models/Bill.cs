namespace Statesman.BillScraper.Data.Models;

public class Bill
{
    // Domain ID
    public int Id { get; set; }

    // Neo4j node ID
    public long NodeId { get; set; }
    public string Title { get; set; } = null!;
    public string Sign { get; set; } = null!;
    public string RawText { get; set; } = null!;
    public DateTime Date { get; set; }
    public bool IsParsed { get; set; } = false;
    public DateTime? ParsedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Legislator> Sponsors { get; set; } = new List<Legislator>();
}
