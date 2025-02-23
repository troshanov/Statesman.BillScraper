using System.Text.Json.Serialization;

namespace Statesman.BillScraper.DTOs;

internal class LegislatorDto
{
    [JsonPropertyName("A_ns_MP_id")]
    public int Id { get; set; }

    [JsonPropertyName("A_ns_MPL_Name1")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("A_ns_MPL_Name2")]
    public string MiddleName { get; set; } = null!;

    [JsonPropertyName("A_ns_MPL_Name3")]
    public string LastName { get; set; } = null!;
}
