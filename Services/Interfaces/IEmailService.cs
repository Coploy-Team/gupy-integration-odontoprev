namespace GupyIntegration.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendJobPostingEmailAsync(string linkVaga, string email, string senha, string nomeVaga);
    }
} 