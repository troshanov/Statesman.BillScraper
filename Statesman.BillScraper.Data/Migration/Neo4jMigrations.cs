namespace Statesman.BillScraper.Data.Migration;

internal class Neo4jMigrations
{
    public static IEnumerable<Migration> GetMigrations()
    {
        yield return new Migration
        {
            Version = "202502100600_InitialConstraints",
            Description = "Initial constraints",
            UpStatements = new List<string>
            {
                @"CREATE CONSTRAINT bill_id_unique IF NOT EXISTS FOR (b:Bill) REQUIRE b.Id IS UNIQUE",

                @"CREATE CONSTRAINT legislator_id_unique IF NOT EXISTS FOR (l:Legislator) REQUIRE l.Id IS UNIQUE"
            }
        };

        yield return new Migration
        {
            Version = "202506081106_BillElementsConstraints",
            Description = "Bill Elements constraints",
            UpStatements = new List<string>
            {
                @"CREATE CONSTRAINT article_id_unique IF NOT EXISTS FOR (a:Article) REQUIRE a.Id IS UNIQUE",

                @"CREATE CONSTRAINT chapter_id_unique IF NOT EXISTS FOR (c:Chapter) REQUIRE c.Id IS UNIQUE",

                @"CREATE CONSTRAINT section_id_unique IF NOT EXISTS FOR (s:Section) REQUIRE s.Id IS UNIQUE",

                @"CREATE CONSTRAINT paragraph_id_unique IF NOT EXISTS FOR (p:Paragraph) REQUIRE p.Id IS UNIQUE",

                @"CREATE CONSTRAINT point_id_unique IF NOT EXISTS FOR (p:Point) REQUIRE p.Id IS UNIQUE",

                @"CREATE CONSTRAINT letter_id_unique IF NOT EXISTS FOR (l:Letter) REQUIRE l.Id IS UNIQUE"
            }
        };
    }
}
