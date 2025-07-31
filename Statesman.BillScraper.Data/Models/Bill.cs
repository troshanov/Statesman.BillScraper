namespace Statesman.BillScraper.Data.Models;

public class Bill : Neo4jEntity
{
    public string Title { get; set; } = null!;
    public string Sign { get; set; } = null!;
    public string RawText { get; set; } = null!;
    public DateTime Date { get; set; }
    public bool IsParsed { get; set; } = false;
    public DateTime? ParsedAt { get; set; }

    // Navigation property
    public ICollection<Legislator> Sponsors { get; set; } = new List<Legislator>();
    public ICollection<BillElement> BillElements { get; set; } = new List<BillElement>();
}
