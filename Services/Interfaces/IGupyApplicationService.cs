using GupyIntegration.Models;

namespace GupyIntegration.Services.Interfaces
{
  public interface IGupyApplicationService
  {
    Task<GupyApplicationResponse> GetApplicationsAsync(long jobId, long? applicationId = null, int page = 1, int perPage = 10, string order = "id asc");
    Task AddTagToApplicationAsync(long jobId, long applicationId, ApplicationTag tag);
  }
}