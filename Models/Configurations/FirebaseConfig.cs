namespace GupyIntegration.Models.Configurations
{
  public class FirebaseConfig
  {
    public required string ProjectId { get; set; }
    public required string PrivateKey { get; set; }
    public required string ClientEmail { get; set; }
    public string CredentialsFile { get; set; } = string.Empty;
  }
}