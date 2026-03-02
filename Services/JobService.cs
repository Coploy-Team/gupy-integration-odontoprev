using GupyIntegration.Models;
using GupyIntegration.Services.Interfaces;

namespace GupyIntegration.Services
{
  public class JobService(IHttpClientFactory httpClientFactory, ILogger<JobService> logger) : IJobService
  {
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("GupyApi");
    private readonly ILogger<JobService> _logger = logger;

    public async Task<JobResponse> GetJobsAsync(long? id = null, int perPage = 10, int page = 1)
    {
      _logger.LogInformation("Iniciando busca de jobs - Id: {Id}, Page: {Page}, PerPage: {PerPage}",
        id, page, perPage);

      try
      {
        var url = "api/v1/jobs?";
        if (id.HasValue)
        {
          url += $"id={id}&";
        }
        url += $"perPage={perPage}&page={page}";

        _logger.LogDebug("Realizando requisição GET para URL: {Url}", url);
        var response = await _httpClient.GetAsync(url);

        _logger.LogDebug("Status code da resposta: {StatusCode}", response.StatusCode);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JobResponse>()
            ?? throw new Exception("Failed to deserialize response from Gupy API");

        _logger.LogInformation("Jobs recuperados com sucesso - Total de registros: {Total}",
            result.Results?.Count ?? 0);

        return result;
      }
      catch (Exception)
      {
        _logger.LogError("Erro ao buscar jobs - Id: {Id}, Page: {Page}, PerPage: {PerPage}",
            id, page, perPage);
        throw;
      }
    }

    public async Task<JobStepResponse> GetJobStepsAsync(long jobId)
    {
      _logger.LogInformation("Buscando steps da vaga - JobId: {JobId}", jobId);

      try
      {
        var response = await _httpClient.GetAsync($"api/v1/jobs/{jobId}/steps");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JobStepResponse>()
            ?? throw new Exception("Failed to deserialize job steps from Gupy API");

        _logger.LogInformation("Steps recuperados com sucesso - JobId: {JobId}, Total: {Total}",
            jobId, result.Results?.Count ?? 0);

        return result;
      }
      catch (Exception)
      {
        _logger.LogError("Erro ao buscar steps da vaga - JobId: {JobId}", jobId);
        throw;
      }
    }
  }
}