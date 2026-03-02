namespace GupyIntegration.Models.Dify
{
  public class DifyQuestionsRequest
  {
    public required string Cargo { get; set; }
    public required string Nivel { get; set; }
    public required string Descricao { get; set; }
    public required string Responsabilidades { get; set; }
    public required string Requisitos { get; set; }
    public required string Criticas { get; set; }
    public required string Adicionais { get; set; }
    public required string Expectativa { get; set; }
    public required int Numero { get; set; }
  }
}