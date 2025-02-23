using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper.Data.Repositories.Interfaces;

public interface ILegislatorRepository
{
    Task<Legislator?> CreateLegislatorAsync(Legislator legislator);
    Task<Legislator?> GetLegislatorByIdAsync(int id);
    Task<Legislator?> GetLegislatorByNodeIdAsync(string nodeId);
    Task<IEnumerable<Bill>> GetSponsoredBillsAsync(int legislatorId);
}
