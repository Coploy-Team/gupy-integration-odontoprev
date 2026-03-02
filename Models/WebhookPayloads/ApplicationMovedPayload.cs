using Newtonsoft.Json;

namespace GupyIntegration.Models.WebhookPayloads
{

  public class ApplicationMovedPayload
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
    public required DataMoved Data { get; set; }
  }

  public class DataMoved
  {
    [JsonProperty("job")]
    public required JobMoved Job { get; set; }

    [JsonProperty("application")]
    public required ApplicationMoved Application { get; set; }

    [JsonProperty("candidate")]
    public required CandidateMoved Candidate { get; set; }

  }

  public class ApplicationMoved
  {
    [JsonProperty("id")]
    public required long Id { get; set; }
    [JsonProperty("status")]
    public required string Status { get; set; }
    [JsonProperty("tags")]
    public required List<string> Tags { get; set; }
    [JsonProperty("currentStep")]
    public StepMoved? CurrentStep { get; set; }
    [JsonProperty("previousStep")]
    public StepMoved? PreviousStep { get; set; }

  }

  public class StepMoved
  {
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
  }

  public class CandidateMoved
  {
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("lastName")]
    public required string LastName { get; set; }

    [JsonProperty("email")]
    public required string Email { get; set; }

    [JsonProperty("phoneNumber")]
    public string? PhoneNumber { get; set; }
    [JsonProperty("mobileNumber")]
    public string? MobileNumber { get; set; }

  }

  public class JobMoved
  {

    [JsonProperty("id")]
    public required long Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("type")]
    public required string Type { get; set; }

    [JsonProperty("branch")]
    public required BranchMoved Branch { get; set; }
  }
  public class BranchMoved
  {
    [JsonProperty("name")]
    public required string Name { get; set; }
  }
}
