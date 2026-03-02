using GupyIntegration.Models.WebhookPayloads;

namespace GupyIntegration.Services.Interfaces
{
  public interface IWebhookService
  {
    Task HandleApplicationMovedAsync(ApplicationMovedPayload payload);
    Task HandleJobPublishedAsync(JobPublishedPayload payload);
    Task ProcessJobByIdAsync(long jobId);

  }
}
