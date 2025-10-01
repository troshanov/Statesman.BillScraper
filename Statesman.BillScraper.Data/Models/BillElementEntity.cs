namespace Statesman.BillScraper.Data.Models;

public abstract class BillElementEntity : Neo4jEntity
{
    protected BillElementEntity(string text, string marker)
    {
        Text = text;
        Marker = marker;
        ChildElements = new List<BillElementEntity>();
    }

    public string Text { get; set; } = null!;
    public string Marker { get; set; } = null!;

    // Navigation propertys
    public BillElementEntity? ParentElement { get; set; }
    public BillEntity? Bill { get; set; }
    public ICollection<BillElementEntity> ChildElements { get; set; } = new List<BillElementEntity>();
}
