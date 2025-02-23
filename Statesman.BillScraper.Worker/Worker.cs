using System.Text;
using System.Net.Http.Json;
using Statesman.BillScraper.DTOs;
using Statesman.BillScraper.Utilities.ImageScanner;
using Statesman.BillScraper.Utilities.ImageExtractor;
using AutoMapper;
using Statesman.BillScraper.Data.Repositories.Interfaces;
using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGoogleCloudVisionService _cloudVisionService;
    private readonly IMapper _mapper;
    private readonly IBillRepository _billRepository;
    private readonly ILegislatorRepository _legislatorRepository;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IGoogleCloudVisionService cloudVisionService,
        IMapper mapper,
        IBillRepository billRepository,
        ILegislatorRepository legislatorRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _cloudVisionService = cloudVisionService;
        _mapper = mapper;
        _billRepository = billRepository;
        _legislatorRepository = legislatorRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var billDtos = await GetBills();

        foreach (var billDto in billDtos)
        {
            var bill = _mapper.Map<Bill>(billDto);

            var pdfBytes = await GetBillPdf(billDto.PdfId);
            var extractionResult = PdfImageExtractor.ExtractImages(pdfBytes);

            var billText = new StringBuilder();
            foreach (var image in extractionResult)
            {
                var text = await _cloudVisionService.ExtractTextFromImage(image.Data);
                billText.AppendLine(text);
            }

            bill.RawText = billText.ToString();
            var savedBill = await _billRepository.CreateBillAsync(bill);

            if (savedBill != null)
            {
                foreach (var legislatorDto in billDto.Legislators)
                {
                    var legislator = _mapper.Map<Legislator>(legislatorDto);

                    var savedLegislator = await _legislatorRepository.GetLegislatorByIdAsync(legislator.Id);

                    if (savedLegislator == null)
                        savedLegislator = await _legislatorRepository.CreateLegislatorAsync(legislator);

                    await _billRepository.AddSponsorToBillAsync(savedBill.Id, savedLegislator.Id);
                }

                _logger.LogInformation("Successfully processed bill {BillId}: {Title}", savedBill.Id, savedBill.Title);
            }
        }
    }

    private async Task<BillDto[]> GetBills()
    {
        var client = _httpClientFactory.CreateClient("parliamentClient");
        var response = await client.PostAsync("/api/v1/fn-bills", null);
        response.EnsureSuccessStatusCode();

        var bills = await response.Content.ReadFromJsonAsync<BillDto[]>();

        return bills!;
    }

    private async Task<byte[]> GetBillPdf(string pdfId)
    {
        var pathId = pdfId.Split("-", StringSplitOptions.RemoveEmptyEntries).First();

        var client = _httpClientFactory.CreateClient("parliamentClient");
        var response = await client.GetAsync($"/bills/{pathId}/{pdfId}.pdf");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
