namespace GupyIntegration.Models.Response
{
  public class CompanyInterviewSimplifiedResponse
  {
    public string Id { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string JobIdentifier { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Email { get; set; } = string.Empty;
  }
}