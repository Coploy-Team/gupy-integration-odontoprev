using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Response
{
  public class CompanyInterviewResponse
  {
    public string CandidateStatus { get; set; } = string.Empty;
    public string CareerLevel { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Timestamp Date { get; set; }
    public Timestamp DateSelect { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public bool Finish { get; set; }
    public string JobAppliedRefId { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JobRefId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string ProfessionalExperience { get; set; } = string.Empty;
    public double Score { get; set; }
    public string State { get; set; } = string.Empty;
    public bool Stopped { get; set; }
    public string TypeInterview { get; set; } = string.Empty;
    public string UserRefId { get; set; } = string.Empty;
    public string? JobIdentifier { get; set; }
  }
}