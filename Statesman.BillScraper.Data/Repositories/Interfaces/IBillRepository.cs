using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper.Data.Repositories.Interfaces;
public interface IBillRepository
{
    Task<Bill?> CreateBillAsync(Bill bill);
    Task<Bill?> GetBillByIdAsync(int id);
    Task<Bill?> GetBillByNodeIdAsync(long nodeId);
    Task AddSponsorToBillAsync(int billId, int legislatorId);
    Task<IEnumerable<Legislator>> GetBillSponsorsAsync(int billId);
    Task<IEnumerable<Bill>> GetUnparsedBillsAsync();
}
