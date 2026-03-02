using System.Text.Json;
using System.Text;
using GupyIntegration.Services.Interfaces;

namespace GupyIntegration.Services
{
    public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, HttpClient httpClient, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendJobPostingEmailAsync(string linkVaga, string email, string senha, string nomeVaga)
    {
        var url = _configuration["EmailIntegration:Url"];
        var apiKey = _configuration["EmailIntegration:ApiKey"];
        var from = _configuration["EmailIntegration:From"];
        var templateId = _configuration.GetValue<int>("EmailIntegration:TemplateEnvioVaga");

        _logger.LogInformation("🔔 Iniciando envio de email - Destinatário: {Email}, Vaga: {NomeVaga}, Link: {LinkVaga}", 
            email, nomeVaga, linkVaga);

        try
        {
            var request = new
            {
                From = from,
                To = email,
                TemplateId = templateId,
                TemplateModel = new
                {
                    linkVaga,
                    email,
                    senha,
                    nomeVaga
                }
            };

            var json = JsonSerializer.Serialize(request);
            _logger.LogDebug("📤 Request Postmark: {Json}", json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Usar HttpRequestMessage para evitar acúmulo de headers no HttpClient singleton
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            httpRequest.Headers.Add("X-Postmark-Server-Token", apiKey);
            
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Erro Postmark - Status: {StatusCode}, Response: {ErrorContent}", 
                    (int)response.StatusCode, errorContent);
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Details: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("✅ Email enviado com sucesso - Destinatário: {Email}, Response: {Response}", 
                email, responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Erro HTTP ao enviar email para {Email} - Vaga: {NomeVaga}", email, nomeVaga);
            throw new Exception($"Erro ao enviar email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro inesperado ao enviar email para {Email} - Vaga: {NomeVaga}", email, nomeVaga);
            throw new Exception($"Erro ao enviar email: {ex.Message}", ex);
        }
    }
    }
} 