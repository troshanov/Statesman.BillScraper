using System.Net.Http.Json;
using Statesman.BillScraper.DTOs;

namespace Statesman.BillScraper.Services;

internal class BillClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public BillClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<BillDto>> GetBillsAsync()
    {
        var client = _httpClientFactory.CreateClient("parliamentClient");
        var response = await client.PostAsync("/api/v1/fn-bills", null);
        response.EnsureSuccessStatusCode();

        var bills = await response.Content.ReadFromJsonAsync<IEnumerable<BillDto>>();

        if (bills == null)
            throw new InvalidOperationException("Failed to deserialize bills from response.");

        return bills;
    }
}