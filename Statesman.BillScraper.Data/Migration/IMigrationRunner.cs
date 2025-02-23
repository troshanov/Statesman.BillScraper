namespace Statesman.BillScraper.Data.Migration;

public interface IMigrationRunner
{
    Task RunMigrationsAsync();
}
