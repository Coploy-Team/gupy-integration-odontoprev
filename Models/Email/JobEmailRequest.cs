namespace GupyIntegration.Models.Email
{
    public class JobEmailRequest
    {
        public required string LinkVaga { get; set; }
        public required string Email { get; set; }
        public required string Senha { get; set; }
        public required string NomeVaga { get; set; }
    }
}