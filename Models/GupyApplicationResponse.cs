

namespace GupyIntegration.Models
{
  public class GupyApplicationResponse
  {
    public List<ApplicationResult> Results { get; set; } = [];
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
  }

  public class ApplicationResult
  {
    public long Id { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = [];
    public Candidate Candidate { get; set; } = new();
    public Job Job { get; set; } = new();
    public object ManualCandidate { get; set; } = new();
    public CurrentStep CurrentStep { get; set; } = new();
  }

  public class Candidate
  {
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentificationDocument { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public string LinkedinProfileUrl { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
  }

  public class Job
  {
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  public class CurrentStep
  {
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
  }
}