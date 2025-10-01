using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper.Data.Repositories.Interfaces;

public interface ILegislatorRepository
{
    Task<LegislatorEntity?> CreateLegislatorAsync(LegislatorEntity legislator);
    Task<LegislatorEntity?> GetLegislatorByIdAsync(int id);
    Task<LegislatorEntity?> GetLegislatorByNodeIdAsync(long nodeId);
    Task<IEnumerable<BillEntity>> GetSponsoredBillsAsync(int legislatorId);
}
