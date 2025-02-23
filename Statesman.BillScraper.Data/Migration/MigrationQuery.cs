namespace Statesman.BillScraper.Data.Migration;

internal class MigrationQuery
{
    public required string Statement { get; set; }
    public required Neo4jQueryType QueryType { get; set; }
}
