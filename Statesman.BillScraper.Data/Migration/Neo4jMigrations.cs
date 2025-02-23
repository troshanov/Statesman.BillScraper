namespace Statesman.BillScraper.Data.Migration;

internal class Neo4jMigrations
{
    public static IEnumerable<Migration> GetMigrations()
    {
        yield return new Migration
        {
            Version = "202502100600_InitialConstraints",
            Description = "Initial constraints",
            UpStatements = new List<MigrationQuery>
            {
                new MigrationQuery{
                    QueryType = Neo4jQueryType.Create,
                    Statement = @"CONSTRAINT bill_id_unique IF NOT EXISTS FOR (b:Bill) REQUIRE b.Id IS UNIQUE"
                },

                new MigrationQuery{
                    QueryType = Neo4jQueryType.Create,
                    Statement = @"CONSTRAINT legislator_id_unique IF NOT EXISTS FOR (l:Legislator) REQUIRE l.Id IS UNIQUE"
                }
            }

        };
    }
}
