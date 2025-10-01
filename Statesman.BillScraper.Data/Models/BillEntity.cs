namespace Statesman.BillScraper.Data.Models;

public class BillEntity : Neo4jEntity
{
    public string Title { get; set; } = null!;
    public string Sign { get; set; } = null!;
    public string RawText { get; set; } = null!;
    public DateTime Date { get; set; }
    public bool IsParsed { get; set; } = false;
    public DateTime? ParsedAt { get; set; }

    // Navigation property
    public ICollection<LegislatorEntity> Sponsors { get; set; } = new List<LegislatorEntity>();
    public ICollection<BillElementEntity> BillElements { get; set; } = new List<BillElementEntity>();

    public void ParseBill() 
    {
        IsParsed = true;
        ParsedAt = DateTime.UtcNow;
    }
}
