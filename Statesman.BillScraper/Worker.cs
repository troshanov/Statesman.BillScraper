using AutoMapper;
using Statesman.BillScraper.Data.Models;
using Statesman.BillScraper.Data.Repositories.Interfaces;
using Statesman.BillScraper.Services;

namespace Statesman.BillScraper;

internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMapper _mapper;
    private readonly IBillRepository _billRepository;
    private readonly TimeSpan _processingInterval;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly BillClientService _billClient;
    private readonly string _parliamentBaseUrl;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IMapper mapper,
        IBillRepository billRepository,
        IHostApplicationLifetime hostApplicationLifetime,
        BillClientService billClient)
    {
        _logger = logger;
        _mapper = mapper;
        _billRepository = billRepository;
        _hostApplicationLifetime = hostApplicationLifetime;
        _billClient = billClient;
        _parliamentBaseUrl = configuration.GetValue<string>("Statesman:ParliamentBaseUrl")!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bill Scraper Worker starting. Processing interval: {Interval}", _processingInterval);

        // Wait for the application to be fully started before beginning work
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        try
        {
            using (_logger.BeginScope("Bill Scraper Worker Execution"))
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Bill Scraper Worker started running");
                    await ProcessAsync(stoppingToken);
                    _logger.LogInformation("Bill Scraper Worker completed processing. Waiting for next interval.");
                    await Task.Delay(_processingInterval, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Bill Scraper Worker. The service will stop.");

            // Signal the application to shut down gracefully
            _hostApplicationLifetime.StopApplication();
        }
        finally
        {
            _logger.LogInformation("Bill Scraper Worker stopped");
        }
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            var billDtos = await _billClient.GetBillsAsync();
            var filterDate = await _billRepository.GetLatestBillDateAsync();
            billDtos = billDtos
                .Where(b => filterDate == null || DateTime.Parse(b.Date).Date > filterDate.Value.Date)
                .ToList();

            foreach (var billDto in billDtos)
            {
                var bill = _mapper.Map<BillEntity>(billDto);

                // Check if bill already exists in database
                var existingBill = await _billRepository.GetBillByIdAsync(bill.Id);
                if (existingBill != null)
                {
                    _logger.LogInformation("Bill with Id {Id} already exists, skipping processing", bill.Id);
                    continue;
                }

                bill.PdfUrl = GetBillPdfUrl(billDto.PdfId);
                var legislators = billDto.Legislators
                    .Select(_mapper.Map<LegislatorEntity>)
                    .ToList();

                var savedBill = await _billRepository.CreateBillWithSponsorsAsync(bill, legislators);

                if (savedBill != null)
                {
                    _logger.LogInformation("Successfully processed bill {BillId} with {SponsorCount} sponsors", 
                        savedBill.Id, legislators.Count);
                }
                else
                {
                    _logger.LogError("Failed to create bill {BillId}", bill.Id);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Fatal error processing bills", ex);
        }
    }

    private string GetBillPdfUrl(string pdfId)
    {
        var pathId = pdfId.Split("-", StringSplitOptions.RemoveEmptyEntries).First();
        return $"{_parliamentBaseUrl}/bills/{pathId}/{pdfId}.pdf";
    }
}
