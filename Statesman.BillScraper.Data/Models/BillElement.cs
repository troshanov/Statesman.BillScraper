namespace Statesman.BillScraper.Data.Models;

public abstract class BillElement : Neo4jEntity
{
    protected BillElement(string text, string marker)
    {
        Text = text;
        Marker = marker;
        ChildElements = new List<BillElement>();
    }

    public string Text { get; set; } = null!;
    public string Marker { get; set; } = null!;

    // Navigation propertys
    public BillElement? ParentElement { get; set; }
    public Bill? Bill { get; set; }
    public ICollection<BillElement> ChildElements { get; set; } = new List<BillElement>();
}
