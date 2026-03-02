using Newtonsoft.Json;

namespace GupyIntegration.Models.WebhookPayloads
{
  public class JobPublishedPayload
  {
    [JsonProperty("companyName")]
    public required string CompanyName { get; set; }

    [JsonProperty("event")]
    public required string Event { get; set; }

    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("date")]
    public required DateTimeOffset Date { get; set; }

    [JsonProperty("data")]
    public required Data Data { get; set; }
  }

  public class Data
  {
    [JsonProperty("id")]
    public required long Id { get; set; }

    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("status")]
    public required string Status { get; set; }

    [JsonProperty("publicationType")]
    public string? PublicationType { get; set; }
  }
}