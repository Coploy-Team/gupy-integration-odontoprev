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

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("responsibilities")]
    public string? Responsibilities { get; set; }

    [JsonProperty("prerequisites")]
    public string? Prerequisites { get; set; }

    [JsonProperty("additionalInformation")]
    public string? AdditionalInformation { get; set; }

    [JsonProperty("addressCity")]
    public string? AddressCity { get; set; }

    [JsonProperty("addressState")]
    public string? AddressState { get; set; }

    [JsonProperty("addressCountry")]
    public string? AddressCountry { get; set; }

    [JsonProperty("workplaceType")]
    public string? WorkplaceType { get; set; }

    [JsonProperty("applicationDeadline")]
    public string? ApplicationDeadline { get; set; }

    [JsonProperty("department")]
    public JobPublishedDepartment? Department { get; set; }

    [JsonProperty("customFields")]
    public List<JobPublishedCustomField>? CustomFields { get; set; }
  }

  public class JobPublishedDepartment
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
  }

  public class JobPublishedCustomField
  {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("value")]
    public string? Value { get; set; }
  }
}