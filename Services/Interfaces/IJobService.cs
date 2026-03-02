using GupyIntegration.Models;

namespace GupyIntegration.Services.Interfaces
{
  public interface IJobService
  {
    Task<JobResponse> GetJobsAsync(long? id = null, int perPage = 10, int page = 1);
    Task<JobStepResponse> GetJobStepsAsync(long jobId);
  }
}