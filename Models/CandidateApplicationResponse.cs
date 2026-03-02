namespace GupyIntegration.Models
{
  public class CandidateApplicationResponse
  {
    public required string Id { get; set; }
    public required string JobId { get; set; }
    public required string CandidateId { get; set; }
    public required string CandidateName { get; set; }
    public required string CandidateEmail { get; set; }
    public required DateTime AppliedAt { get; set; }
  }
}