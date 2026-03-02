using System.Text.Json.Serialization;

namespace GupyIntegration.Models.Dify
{
  public class DifyJobResponse
  {
    [JsonPropertyName("competencias_criticas")]
    public required string CompetenciasCriticas { get; set; }
    [JsonPropertyName("competencias_adicionais")]
    public required string CompetenciasAdicionais { get; set; }
    [JsonPropertyName("expectativa")]
    public required string Expectativa { get; set; }
  }
}