namespace GupyIntegration.Models.Dify
{
  public class DifyJobRequest
  {
    public string Cargo { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Requisitos { get; set; } = null!;
    public string Responsabilidades { get; set; } = null!;
    public string Idioma { get; set; } = "pt_BR";
  }
}