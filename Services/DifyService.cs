using GupyIntegration.Models.Dify;
using GupyIntegration.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GupyIntegration.Services
{
  public class DifyService : IDifyService
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DifyService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _baseUrl = configuration["DifyService:BaseUrl"]
          ?? throw new ArgumentNullException(nameof(configuration), "DifyService:BaseUrl not configured");
      _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<DifyJobResponse> GenerateSkillsJobAsync(DifyJobRequest request)
    {
      try
      {
        var response = await _httpClient.PostAsJsonAsync("/api/v2/dify/generate-skills-job", request);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DifyJobResponse>()
            ?? throw new Exception("Failed to deserialize Dify API response");
      }
      catch (Exception ex)
      {
        throw new Exception($"Erro ao gerar skills do job: {ex.Message}", ex);
      }
    }

    public async Task<List<string>> GenerateQuestionsAsync(DifyQuestionsRequest request)
    {
      try
      {
        var response = await _httpClient.PostAsJsonAsync("/api/v2/dify/generate-questions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<string>()
            ?? throw new Exception("Failed to deserialize questions response");
        var questionsArray = ExtractQuestionsFromResult(result);
        return questionsArray;
      }
      catch (Exception ex)
      {
        throw new Exception($"Erro ao gerar perguntas do job: {ex.Message}", ex);
      }
    }

    private static List<string> ExtractQuestionsFromResult(string result)
    {
      try
      {
        // Parse JSON string
        var jsonObject = JObject.Parse(result);

        // Extract only the values into an array
        var questions = jsonObject.Properties()
            .Select(p => p.Value.ToString())
            .ToArray();

        return questions.ToList();
      }
      catch (JsonReaderException ex)
      {
        throw new Exception("Failed to parse questions JSON", ex);
      }
    }
  }
}