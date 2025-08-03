namespace Statesman.BillScraper.Data.Migration;
internal class Migration
{
    public required string Version { get; init; }
    public required string Description { get; init; }
    public List<string> UpStatements { get; set; } = new();
}
