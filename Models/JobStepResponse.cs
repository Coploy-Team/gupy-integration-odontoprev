using System.Text.Json.Serialization;

namespace GupyIntegration.Models
{
  public class JobStepResponse
  {
    [JsonPropertyName("results")]
    public List<JobStep> Results { get; set; } = [];

    [JsonPropertyName("totalResults")]
    public long TotalResults { get; set; }

    [JsonPropertyName("page")]
    public long Page { get; set; }

    [JsonPropertyName("totalPages")]
    public long TotalPages { get; set; }
  }

  public class JobStep
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
  }
}
