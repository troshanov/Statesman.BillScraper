using System.Text.Json.Serialization;

namespace Statesman.BillScraper.DTOs;

internal class BillDto
{
    [JsonPropertyName("L_Act_id")]
    public int Id { get; set; }

    [JsonPropertyName("L_Act_sign")]
    public string PdfId { get; set; } = null!;

    [JsonPropertyName("L_Act_date")]
    public string Date { get; set; } = null!;

    [JsonPropertyName("L_ActL_title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("imp_list")]
    public List<LegislatorDto> Legislators { get; set; } = new();
}
