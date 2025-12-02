using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;

namespace Statesman.BillScraper.Data.Repositories;

public class Neo4jBillRepository : IBillRepository
{
    private readonly IDriver _driver;
    private readonly string _database;
    public Neo4jBillRepository(IDriver driver, IConfiguration configuration)
    {
        _driver = driver;
        _database = configuration.GetSection("Neo4j")["Database"]!;
    }

    public async Task<BillEntity?> CreateBillAsync(BillEntity bill)
    {
        try
        {
            await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var query = @"
                    CREATE (b:Bill {
                        Id: $id,
                        Title: $title,
                        Sign: $sign,
                        PdfUrl: $pdfUrl,
                        Date: $date,
                        IsParsed: $isParsed,
                        ParsedAt: $parsedAt,
                        UpdatedAt: $updatedAt
                    })
                    RETURN b, ID(b) as nodeId";

                var cursor = await tx.RunAsync(query, new
                {
                    id = bill.Id,
                    title = bill.Title,
                    sign = bill.Sign,
                    pdfUrl = bill.PdfUrl,
                    date = bill.Date,
                    isParsed = bill.IsParsed,
                    parsedAt = bill.ParsedAt,
                    updatedAt = bill.UpdatedAt
                });

                var record = await cursor.SingleAsync();
                var node = record["b"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return MapNodeToBill(node, nodeId);
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bill: {ex.Message}");
            return null;
        }
    }

    public async Task<BillEntity?> CreateBillWithSponsorsAsync(BillEntity bill, IEnumerable<LegislatorEntity> legislators)
    {
        try
        {
            await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                // 1. Create the bill
                var billQuery = @"
                    CREATE (b:Bill {
                        Id: $id,
                        Title: $title,
                        Sign: $sign,
                        PdfUrl: $pdfUrl,
                        Date: $date,
                        IsParsed: $isParsed,
                        ParsedAt: $parsedAt,
                        UpdatedAt: $updatedAt
                    })
                    RETURN b, ID(b) as nodeId";

                var billCursor = await tx.RunAsync(billQuery, new
                {
                    id = bill.Id,
                    title = bill.Title,
                    sign = bill.Sign,
                    pdfUrl = bill.PdfUrl,
                    date = bill.Date,
                    isParsed = bill.IsParsed,
                    parsedAt = bill.ParsedAt,
                    updatedAt = bill.UpdatedAt
                });

                var billRecord = await billCursor.SingleAsync();
                var billNode = billRecord["b"].As<INode>();
                var billNodeId = billRecord["nodeId"].As<long>();

                // 2. Process each legislator and create sponsor relationship
                foreach (var legislator in legislators)
                {
                    // MERGE legislator (create if doesn't exist, or match if it does)
                    var legislatorQuery = @"
                        MERGE (l:Legislator {Id: $id})
                        ON CREATE SET 
                            l.FirstName = $firstName,
                            l.MiddleName = $middleName,
                            l.LastName = $lastName
                        RETURN l, ID(l) as nodeId";

                    await tx.RunAsync(legislatorQuery, new
                    {
                        id = legislator.Id,
                        firstName = legislator.FirstName,
                        middleName = legislator.MiddleName,
                        lastName = legislator.LastName
                    });

                    // 3. Create sponsor relationship
                    var sponsorQuery = @"
                        MATCH (b:Bill {Id: $billId}), (l:Legislator {Id: $legislatorId})
                        CREATE (l)-[:SPONSORS]->(b)";

                    await tx.RunAsync(sponsorQuery, new
                    {
                        billId = bill.Id,
                        legislatorId = legislator.Id
                    });
                }

                return MapNodeToBill(billNode, billNodeId);
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bill with sponsors: {ex.Message}");
            return null;
        }
    }

    public async Task<BillEntity?> GetBillByIdAsync(int id)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {Id: $id})
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query, new { id });
            var records = await cursor.ToListAsync();

            if (!records.Any())
                return null;

            var record = records.First();
            var node = record["b"].As<INode>();
            var nodeId = record["nodeId"].As<long>();

            return MapNodeToBill(node, nodeId);
        });
    }

    public async Task AddSponsorToBillAsync(int billId, int legislatorId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        await session.ExecuteWriteAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {Id: $billId}), (l:Legislator {Id: $legislatorId})
                CREATE (l)-[:SPONSORS]->(b)";

            await tx.RunAsync(query, new { billId, legislatorId });
        });
    }

    public async Task<IEnumerable<LegislatorEntity>> GetBillSponsorsAsync(int billId)
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (l:Legislator)-[:SPONSORS]->(b:Bill {Id: $billId})
                RETURN l, ID(l) as nodeId";

            var cursor = await tx.RunAsync(query, new { billId });
            var records = await cursor.ToListAsync();

            return records.Select(record =>
            {
                var node = record["l"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return new LegislatorEntity
                {
                    Id = node.Properties["Id"].As<int>(),
                    FirstName = node.Properties["FirstName"].As<string>(),
                    MiddleName = node.Properties["MiddleName"].As<string>(),
                    LastName = node.Properties["LastName"].As<string>(),
                    NodeId = nodeId
                };
            }).ToList();
        });
    }

    public async Task<IEnumerable<BillEntity>> GetUnparsedBillsAsync()
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill {IsParsed: false})
                RETURN b, ID(b) as nodeId";

            var cursor = await tx.RunAsync(query);
            var records = await cursor.ToListAsync();

            return records.Select(record =>
            {
                var node = record["b"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                return new BillEntity
                {
                    Id = node.Properties["Id"].As<int>(),
                    Title = node.Properties["Title"].As<string>(),
                    Date = node.Properties["Date"].As<DateTime>(),
                    NodeId = nodeId
                };
            }).ToList();
        });
    }

    public async Task<DateTime?> GetLatestBillDateAsync()
    {
        await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

        return await session.ExecuteReadAsync(async tx =>
        {
            var query = @"
                MATCH (b:Bill)
                RETURN MAX(b.Date) as latestDate";

            var cursor = await tx.RunAsync(query);
            var record = await cursor.SingleAsync();
            
            try
            {
                return record["latestDate"].As<DateTime?>();
            }
            catch
            {
                return null;
            }
        });
    }

    private BillEntity MapNodeToBill(INode node, long nodeId)
    {
        return new BillEntity
        {
            Id = node.Properties["Id"].As<int>(),
            Title = node.Properties["Title"].As<string>(),
            Sign = node.Properties["Sign"].As<string>(),
            PdfUrl = node.Properties["PdfUrl"].As<string>(),
            Date = node.Properties["Date"].As<DateTime>(),
            IsParsed = node.Properties["IsParsed"].As<bool>(),
            ParsedAt = node.Properties.ContainsKey("ParsedAt") ? node.Properties["ParsedAt"].As<DateTime?>() : null,
            UpdatedAt = node.Properties["UpdatedAt"].As<DateTime>(),
            NodeId = nodeId
        };
    }

    // BillElement persistence methods
    public async Task<ArticleEntity?> CreateArticleAsync(ArticleEntity article, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(article, "Article", billId, parentElementNodeId);

    public async Task<ChapterEntity?> CreateChapterAsync(ChapterEntity chapter, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(chapter, "Chapter", billId, parentElementNodeId);

    public async Task<SectionEntity?> CreateSectionAsync(SectionEntity section, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(section, "Section", billId, parentElementNodeId);

    public async Task<ParagraphEntity?> CreateParagraphAsync(ParagraphEntity paragraph, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(paragraph, "Paragraph", billId, parentElementNodeId);

    public async Task<LetterEntity?> CreateLetterAsync(LetterEntity letter, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(letter, "Letter", billId, parentElementNodeId);

    public async Task<PointEntity?> CreatePointAsync(PointEntity point, int billId, long? parentElementNodeId = null) 
        => await CreateBillElementAsync(point, "Point", billId, parentElementNodeId);

    private async Task<T?> CreateBillElementAsync<T>(T element, string label, int billId, long? parentElementNodeId = null)
        where T : BillElementEntity
    {
        try
        {
            await using var session = _driver.AsyncSession(s => s.WithDatabase(_database));

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                // Create the element node
                var createQuery = $@"
                    CREATE (e:{label}:BillElement {{
                        Id: $id,
                        Text: $text,
                        Marker: $marker,
                        UpdatedAt: $updatedAt
                    }})
                    RETURN e, ID(e) as nodeId";

                var cursor = await tx.RunAsync(createQuery, new
                {
                    id = element.Id,
                    text = element.Text,
                    marker = element.Marker,
                    updatedAt = element.UpdatedAt
                });

                var record = await cursor.SingleAsync();
                var node = record["e"].As<INode>();
                var nodeId = record["nodeId"].As<long>();

                // Create relationship to Bill
                var billRelQuery = @"
                    MATCH (b:Bill {Id: $billId})
                    MATCH (e:BillElement)
                    WHERE ID(e) = $elementNodeId
                    CREATE (b)-[:HAS_ELEMENT]->(e)";

                await tx.RunAsync(billRelQuery, new { billId, elementNodeId = nodeId });

                // Create relationship to parent element if provided
                if (parentElementNodeId.HasValue)
                {
                    var parentRelQuery = @"
                        MATCH (parent:BillElement)
                        WHERE ID(parent) = $parentNodeId
                        MATCH (child:BillElement)
                        WHERE ID(child) = $childNodeId
                        CREATE (parent)-[:HAS_CHILD]->(child)";

                    await tx.RunAsync(parentRelQuery, new
                    {
                        parentNodeId = parentElementNodeId.Value,
                        childNodeId = nodeId
                    });
                }

                return MapNodeToBillElement<T>(node, nodeId, element.Text, element.Marker);
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating {label}: {ex.Message}");
            return null;
        }
    }

    private T MapNodeToBillElement<T>(INode node, long nodeId, string text, string marker)
        where T : BillElementEntity
    {
        var element = (T)Activator.CreateInstance(typeof(T), text, marker)!;
        element.Id = node.Properties["Id"].As<int>();
        element.Text = node.Properties["Text"].As<string>();
        element.Marker = node.Properties["Marker"].As<string>();
        element.UpdatedAt = node.Properties["UpdatedAt"].As<DateTime>();
        element.NodeId = nodeId;
        return element;
    }
}
