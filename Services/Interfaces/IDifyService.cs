using GupyIntegration.Models.Dify;

namespace GupyIntegration.Services.Interfaces
{
  public interface IDifyService
  {
    Task<DifyJobResponse> GenerateSkillsJobAsync(DifyJobRequest request);
    Task<List<string>> GenerateQuestionsAsync(DifyQuestionsRequest request);
  }
}