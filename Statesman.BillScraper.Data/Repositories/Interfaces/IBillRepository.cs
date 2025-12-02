using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper.Data.Repositories.Interfaces;
public interface IBillRepository
{
    Task<BillEntity?> CreateBillAsync(BillEntity bill);
    Task<BillEntity?> CreateBillWithSponsorsAsync(BillEntity bill, IEnumerable<LegislatorEntity> legislators);
    Task<BillEntity?> GetBillByIdAsync(int id);
    Task AddSponsorToBillAsync(int billId, int legislatorId);
    Task<IEnumerable<LegislatorEntity>> GetBillSponsorsAsync(int billId);
    Task<IEnumerable<BillEntity>> GetUnparsedBillsAsync();
    Task<DateTime?> GetLatestBillDateAsync();

    // BillElement persistence methods
    Task<ArticleEntity?> CreateArticleAsync(ArticleEntity article, int billId, long? parentElementNodeId = null);
    Task<ChapterEntity?> CreateChapterAsync(ChapterEntity chapter, int billId, long? parentElementNodeId = null);
    Task<SectionEntity?> CreateSectionAsync(SectionEntity section, int billId, long? parentElementNodeId = null);
    Task<ParagraphEntity?> CreateParagraphAsync(ParagraphEntity paragraph, int billId, long? parentElementNodeId = null);
    Task<LetterEntity?> CreateLetterAsync(LetterEntity letter, int billId, long? parentElementNodeId = null);
    Task<PointEntity?> CreatePointAsync(PointEntity point, int billId, long? parentElementNodeId = null);
}
