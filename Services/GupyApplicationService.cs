using GupyIntegration.Models;
using GupyIntegration.Services.Interfaces;


namespace GupyIntegration.Services
{
  public class GupyApplicationService : IGupyApplicationService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<JobService> _logger;

    public GupyApplicationService(IHttpClientFactory httpClientFactory, ILogger<JobService> logger)
    {
      _httpClient = httpClientFactory.CreateClient("GupyApi");
      _logger = logger;
    }

    public async Task<GupyApplicationResponse> GetApplicationsAsync(long jobId, long? applicationId = null, int page = 1, int perPage = 10, string order = "id asc")
    {
      _logger.LogInformation("Buscando aplicações do Gupy - JobId: {JobId}, ApplicationId: {ApplicationId}, Page: {Page}, PerPage: {PerPage}",
          jobId, applicationId, page, perPage);

      var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "perPage", perPage.ToString() },
                { "order", order }
            };

      if (applicationId.HasValue)
      {
        queryParameters.Add("id", applicationId.Value.ToString());
      }

      var queryString = string.Join("&", queryParameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

      try
      {
        var response = await _httpClient.GetAsync($"api/v1/jobs/{jobId}/applications?{queryString}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GupyApplicationResponse>()
            ?? throw new Exception("Failed to deserialize response from Gupy API");
        _logger.LogInformation("Aplicações recuperadas com sucesso - JobId: {JobId}, Total de registros: {Total}",
            jobId, result.Results?.Count ?? 0);

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar aplicações do Gupy - JobId: {JobId}, ApplicationId: {ApplicationId}",
            jobId, applicationId);
        throw;
      }
    }

    public async Task AddTagToApplicationAsync(long jobId, long applicationId, ApplicationTag tag)
    {
      _logger.LogInformation("Adicionando tag à aplicação - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}",
          jobId, applicationId, tag.Name);

      try
      {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/jobs/{jobId}/applications/{applicationId}/tags",
            tag);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Tag adicionada com sucesso - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}",
            jobId, applicationId, tag.Name);
      }
      catch (Exception ex)
      {
        _logger.LogInformation(ex, "Erro ao adicionar tag - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}",
            jobId, applicationId, tag.Name);
        throw;
      }
    }
  }
}