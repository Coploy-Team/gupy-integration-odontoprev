using GupyIntegration.Models.Firebase;
using GupyIntegration.Models.Response;

namespace GupyIntegration.Services.Interfaces
{
  public interface ICompanyInterviewService
  {
    Task<List<CompanyInterviewResponse>> GetCompanyInterviewsAsync();
    Task<CompanyInterviewResponse?> GetCompanyInterviewByIdAsync(string interviewId);
    Task<List<string>> GetSimplifiedInterviewsAsync();
    Task<List<InterviewQuestionsResponse>> GetQuestionsAddicionalAsync();
  }
}