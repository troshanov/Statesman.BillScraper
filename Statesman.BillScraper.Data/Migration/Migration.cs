namespace Statesman.BillScraper.Data.Migration;
internal class Migration
{
    public required string Version { get; init; }
    public required string Description { get; init; }
    public List<MigrationQuery> UpStatements { get; set; } = new();
}
