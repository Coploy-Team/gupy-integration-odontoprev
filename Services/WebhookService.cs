using GupyIntegration.Services.Interfaces;
using GupyIntegration.Models.WebhookPayloads;
using GupyIntegration.Models.Dify;
using GupyIntegration.Services.Mappers;
using GupyIntegration.Models;
using GupyIntegration.Models.Firebase;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace GupyIntegration.Services
{
  public class WebhookService : IWebhookService
  {
    private readonly IJobService _jobService;
    private readonly IFirebaseService _firebaseService;
    private readonly IDifyService _difyService;
    private readonly ILogger<WebhookService> _logger;
    private readonly IGupyApplicationService _gupyApplicationService;
    private readonly IEmailService _emailService;
    private readonly string _companyId;

    public WebhookService(
        IJobService jobService,
        IFirebaseService firebaseService,
        IDifyService difyService,
        IGupyApplicationService gupyApplicationService,
        IEmailService emailService,
        ILogger<WebhookService> logger,
        IConfiguration configuration)
    {
      _jobService = jobService;
      _firebaseService = firebaseService;
      _difyService = difyService;
      _gupyApplicationService = gupyApplicationService;
      _logger = logger;
      _emailService = emailService;
      _companyId = configuration.GetValue<string>("Company:DefaultCompanyId")
          ?? throw new InvalidOperationException("DefaultCompanyId not configured");
    }

    public async Task HandleApplicationMovedAsync(ApplicationMovedPayload payload)
    {
      _logger.LogInformation("🎯 Iniciando processamento de candidatura movida - JobId: {JobId}, ApplicationId: {ApplicationId}, Step: {CurrentStep} -> {PreviousStep}",
          payload.Data.Job.Id, payload.Data.Application.Id, 
          payload.Data.Application.CurrentStep?.Name, 
          payload.Data.Application.PreviousStep?.Name);

      try
      {
        if (payload?.Data == null)
        {
          _logger.LogError("❌ Payload de candidatura inválido - payload ou data são nulos");
          throw new ArgumentNullException(nameof(payload), "Payload não pode ser nulo");
        }

        _logger.LogDebug("Validando dados do webhook - JobId: {JobId}, ApplicationId: {ApplicationId}",
            payload.Data.Job.Id, payload.Data.Application.Id);

        var application = payload.Data.Application;
        if (application == null)
        {
          _logger.LogError("❌ Aplicação não encontrada - JobId: {JobId}, ApplicationId: {ApplicationId}",
              payload.Data.Job.Id, payload.Data.Application.Id);
          throw new InvalidOperationException($"Aplicação não encontrada - JobId: {payload.Data.Job.Id}, ApplicationId: {payload.Data.Application.Id}");
        }
        
        if (payload.Data.Candidate == null)
        {
          _logger.LogError("❌ Dados do candidato não encontrados - JobId: {JobId}, ApplicationId: {ApplicationId}",
              payload.Data.Job.Id, payload.Data.Application.Id);
          throw new InvalidOperationException($"Candidate data not found - JobId: {payload.Data.Job.Id}, ApplicationId: {payload.Data.Application.Id}");
        }
        
        var candidateEmail = payload.Data.Candidate.Email;
        _logger.LogInformation("👨‍💼 Candidato identificado - Nome: {Nome} {Sobrenome}, Email: {Email}",
            payload.Data.Candidate.Name, payload.Data.Candidate.LastName, candidateEmail);

        // Verifica se o step atual é "Entrevista Coploy"
        var currentStepName = payload.Data.Application.CurrentStep?.Name;
        if (!string.Equals(currentStepName, "Entrevista Coploy", StringComparison.OrdinalIgnoreCase))
        {
          _logger.LogInformation("Step atual '{StepName}' não é 'Entrevista Coploy' - ignorando envio de email - JobId: {JobId}, ApplicationId: {ApplicationId}",
              currentStepName, payload.Data.Job.Id, payload.Data.Application.Id);
          return;
        }

        // STEP 1: Valida se a vaga existe no Firebase (PRIMEIRO!)
        _logger.LogDebug("🔍 Verificando se vaga existe no Firebase - JobId: {JobId}", payload.Data.Job.Id);
        var job = await _firebaseService.GetJobByIdentifierAsync(_companyId, payload.Data.Job.Id.ToString());
        
        if (job == null)
        {
          _logger.LogError("❌ Vaga não encontrada no Firebase - JobId: {JobId}. Email não será enviado.", 
            payload.Data.Job.Id);
          return;
        }
        
        _logger.LogInformation("✅ Vaga encontrada no Firebase - JobId: {JobId}, Firebase ID: {FirebaseId}", 
          payload.Data.Job.Id, job.Id);

        // STEP 2: Valida se a tag já foi enviada (SEGUNDO!)
        if (payload.Data.Application.Tags != null && payload.Data.Application.Tags.Any(x => x.Contains("teste-enviado-coploy")))
        {
          _logger.LogInformation("✅ Tag 'teste-enviado-coploy' já existe - Email já foi enviado anteriormente - JobId: {JobId}, ApplicationId: {ApplicationId}",
              payload.Data.Job.Id, payload.Data.Application.Id);
          return;
        }

        _logger.LogDebug("Verificando se usuário existe no Firebase - Email: {Email}", candidateEmail);
        var userExists = await _firebaseService.CheckUserExistsByEmailAsync(candidateEmail);

        // STEP 3: Cria usuário se não existir E envia email
        if (userExists == null)
        {
          var candidateName = $"{payload.Data.Candidate.Name} {payload.Data.Candidate.LastName}";
          _logger.LogInformation("🆕 Criando novo usuário no Firebase - Email: {Email}, Nome: {Nome}", 
            candidateEmail, candidateName);
          
          var defaultPassword = GenerateRandomPassword();
          _logger.LogInformation("🔑 Senha gerada para novo usuário - Email: {Email}", candidateEmail);
          
          var phoneNumber = payload.Data.Candidate.PhoneNumber ?? payload.Data.Candidate.MobileNumber;
          
          await _firebaseService.CreateUserAsync(candidateEmail, defaultPassword, candidateName, payload.Data.Application.Id.ToString(), phoneNumber ?? "");
          _logger.LogInformation("✅ Usuário criado com sucesso no Firebase - Email: {Email}", candidateEmail);
          
          // Tenta enviar email - só adiciona tag se tiver sucesso
          try
          {
            await SendEmail(payload, candidateEmail, defaultPassword);
            
            // SÓ adiciona tag se email foi enviado com sucesso
            await _gupyApplicationService.AddTagToApplicationAsync(payload.Data.Job.Id, payload.Data.Application.Id, new ApplicationTag
            {
              Name = "teste-enviado-coploy",
            });
            _logger.LogInformation("✅ Tag 'teste-enviado-coploy' adicionada com sucesso - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "❌ Erro ao enviar email - Tag NÃO será adicionada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
            throw;
          }
        }
        else
        {
          _logger.LogInformation("👤 Usuário já existe no Firebase - Email: {Email}, UserId: {UserId}", 
            candidateEmail, userExists.Id);

          _logger.LogInformation("🔄 Atualizando ExternalId do usuário - UserId: {UserId}, ExternalId: {ExternalId}", 
            userExists.Id, payload.Data.Application.Id);
          
          userExists.ExternalId = payload.Data.Application.Id.ToString();
          
          // Tenta enviar email - só adiciona tag se tiver sucesso
          try
          {
            _logger.LogInformation("🔁 Enviando email para usuário existente - Email: {Email}", candidateEmail);
            
            await SendEmail(payload, candidateEmail, "Mesma senha já ultilizada na coploy");
            
            _logger.LogInformation("✅ Email enviado com sucesso para usuário existente - Email: {Email}", candidateEmail);
            
            // SÓ adiciona tag se email foi enviado com sucesso
            await _gupyApplicationService.AddTagToApplicationAsync(payload.Data.Job.Id, payload.Data.Application.Id, new ApplicationTag
            {
              Name = "teste-enviado-coploy",
            });
            
            _logger.LogInformation("✅ Tag 'teste-enviado-coploy' adicionada com sucesso - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "❌ Erro ao enviar email - Tag NÃO será adicionada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
            throw;
          }
          
          //atualiza o externalId do usuário
          _logger.LogDebug("Atualizando ExternalId no Firebase - Path: users/{UserId}, ExternalId: {ExternalId}", 
            userExists.Id, userExists.ExternalId);
          
          await _firebaseService.UpdateUserExternalIdAsync($"users/{userExists.Id}", userExists.ExternalId ?? "0");
          //busca a entrevista do usuário
          var path = $"companies/{_companyId}/postJob/{job.Id}/interviews";
          var interviews = await _firebaseService.GetCollectionByPathAsync<Interviews>(path);
          var interview = interviews?.FirstOrDefault(x => x.UserRef?.Id == userExists.Id);
          if (interview == null)
          {
            _logger.LogError("Entrevista não encontrada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
            return;
          }
          
          var jobAppliedPath = interview.JobApplied?.Path?.Replace("projects/coployf/databases/(default)/documents/", string.Empty);
          if (string.IsNullOrEmpty(jobAppliedPath))
          {
            _logger.LogError("Path do JobApplied inválido - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
            return;
          }
          
          var userApplied = await _firebaseService.GetByFullPathAsync<JobApplied>(jobAppliedPath);
          if (userApplied == null)
          {
            _logger.LogError("Aplicação não encontrada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                payload.Data.Job.Id, payload.Data.Application.Id);
            return;
          }



          switch (application.Status)
          {
            case "reproved":
              _logger.LogInformation("Candidatura reprovada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                  payload.Data.Job.Id, payload.Data.Application.Id);
              userApplied.CandidateStatus = "Rejected";
              break;
            case "hired":
              _logger.LogInformation("Candidatura aprovada - JobId: {JobId}, ApplicationId: {ApplicationId}",
                  payload.Data.Job.Id, payload.Data.Application.Id);
              userApplied.CandidateStatus = "Approved";
              break;
          }
          if (userApplied.CandidateStatus == "Approved" || userApplied.CandidateStatus == "Rejected")
          {
            var pathUpdate = $"users/{userExists.Id}/jobsApplied/{userApplied.Id}";
            var updates = new Dictionary<string, object>
            {
              { "candidateStatus", userApplied.CandidateStatus },
            };

            await _firebaseService.UpdatePartialAsync(pathUpdate, updates);
          }
          userExists.ExternalId = payload.Data.Application.Id.ToString();
          await _firebaseService.UpdateUserExternalIdAsync($"users/{userExists.Id}", userExists.ExternalId ?? "0");

        }

        _logger.LogInformation("✅ Candidatura processada com sucesso - JobId: {JobId}, ApplicationId: {ApplicationId}",
            payload.Data.Job.Id, payload.Data.Application.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "❌ ERRO ao processar candidatura movida - JobId: {JobId}, ApplicationId: {ApplicationId}, Candidato: {Email}, Mensagem: {Message}", 
            payload.Data?.Job?.Id, 
            payload.Data?.Application?.Id,
            payload.Data?.Candidate?.Email,
            ex.Message);
        throw;
      }
    }
    public async Task HandleJobPublishedAsync(JobPublishedPayload payload)
    {
      _logger.LogInformation("Iniciando processamento de vaga publicada - JobId: {JobId}, Title: {Title}",
          payload.Data.Id, payload.Data?.Name);

      try
      {
        ValidatePayload(payload);

        if (string.Equals(payload.Data.PublicationType, "internal", StringComparison.OrdinalIgnoreCase))
        {
          _logger.LogInformation("Vaga ignorada por ser do tipo 'internal' - JobId: {JobId}", payload.Data.Id);
          return;
        }

        var jobId = payload?.Data?.Id ?? 0;
        var steps = await _jobService.GetJobStepsAsync(jobId);
        var hasEntrevistaCoployStep = steps.Results?.Any(s =>
            string.Equals(s.Name, "Entrevista Coploy", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (!hasEntrevistaCoployStep)
        {
          _logger.LogInformation("Vaga ignorada - não possui etapa 'Entrevista Coploy' - JobId: {JobId}", jobId);
          return;
        }

        var job = await GetAndValidateJob(jobId);
        var jobData = job.Results?.FirstOrDefault(x => x.Id == payload?.Data?.Id)
            ?? throw new InvalidOperationException($"Dados específicos não encontrados para o JobId: {payload?.Data?.Id}");

        var skillsResponse = await GenerateSkills(jobData, payload?.Data?.Name ?? string.Empty);
        var questions = await GenerateQuestions(jobData, payload?.Data?.Name ?? string.Empty, skillsResponse);

        var postJob = JobMapper.MapToPostJob(jobData, payload?.Data?.Name ?? string.Empty, skillsResponse, questions);

        await _firebaseService.SaveJobPostingAsync(_companyId, postJob);

        _logger.LogInformation("Vaga processada e salva com sucesso - JobId: {JobId}", payload?.Data?.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao processar vaga - JobId: {JobId}", payload?.Data?.Id);
        throw;
      }
    }

    public async Task ProcessJobByIdAsync(long jobId)
    {
      _logger.LogInformation("Processamento manual de vaga iniciado - JobId: {JobId}", jobId);

      try
      {
        var steps = await _jobService.GetJobStepsAsync(jobId);
        var hasEntrevistaCoployStep = steps.Results?.Any(s =>
            string.Equals(s.Name, "Entrevista Coploy", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (!hasEntrevistaCoployStep)
        {
          _logger.LogInformation("Vaga ignorada - não possui etapa 'Entrevista Coploy' - JobId: {JobId}", jobId);
          return;
        }

        var job = await GetAndValidateJob(jobId);
        var jobData = job.Results?.FirstOrDefault(x => x.Id == jobId)
            ?? throw new InvalidOperationException($"Dados específicos não encontrados para o JobId: {jobId}");

        var title = !string.IsNullOrEmpty(jobData.Name) ? jobData.Name : $"Vaga {jobId}";

        var skillsResponse = await GenerateSkills(jobData, title);
        var questions = await GenerateQuestions(jobData, title, skillsResponse);

        var postJob = JobMapper.MapToPostJob(jobData, title, skillsResponse, questions);

        await _firebaseService.SaveJobPostingAsync(_companyId, postJob);

        _logger.LogInformation("Vaga processada manualmente e salva com sucesso - JobId: {JobId}", jobId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao processar vaga manualmente - JobId: {JobId}", jobId);
        throw;
      }
    }

    private void ValidatePayload(JobPublishedPayload payload)
    {
      if (payload?.Data == null)
      {
        _logger.LogError("Payload de vaga inválido - payload ou data são nulos");
        throw new ArgumentNullException(nameof(payload), "Payload não pode ser nulo");
      }

      if (string.IsNullOrEmpty(payload.Data.Id.ToString()))
      {
        throw new ArgumentException("JobId não pode ser nulo ou vazio", nameof(payload));
      }
    }

    private async Task<JobResponse> GetAndValidateJob(long jobId)
    {
      _logger.LogInformation("Buscando dados da vaga na API do Gupy - JobId: {JobId}", jobId);
      var job = await _jobService.GetJobsAsync(jobId);

      if (job == null)
      {
        throw new InvalidOperationException($"Nenhum dado encontrado para o JobId: {jobId}");
      }

      return job;
    }

    private async Task<DifyJobResponse> GenerateSkills(Result jobData, string title)
    {
      var skillsRequest = new DifyJobRequest
      {
        Cargo = title,
        Nivel = JobMapper.GetCareerLevel(jobData.CustomFields),
        Descricao = jobData.Description ?? "Descrição não disponível",
        Requisitos = jobData.Prerequisites ?? "Pré-requisitos não disponíveis",
        Responsabilidades = jobData.Responsibilities ?? "Responsabilidades não disponíveis"
      };

      _logger.LogInformation("Gerando skills via Dify - JobId: {JobId}", jobData.Id);
      return await _difyService.GenerateSkillsJobAsync(skillsRequest);
    }

    private async Task<List<string>> GenerateQuestions(Result jobData, string title, DifyJobResponse skillsResponse)
    {
      var questionsRequest = new DifyQuestionsRequest
      {
        Cargo = title,
        Nivel = JobMapper.GetCareerLevel(jobData.CustomFields),
        Descricao = jobData.Description ?? "Descrição não disponível",
        Requisitos = jobData.Prerequisites ?? "Pré-requisitos não disponíveis",
        Responsabilidades = jobData.Responsibilities ?? "Responsabilidades não disponíveis",
        Criticas = skillsResponse?.CompetenciasCriticas ?? string.Empty,
        Adicionais = skillsResponse?.CompetenciasAdicionais ?? string.Empty,
        Expectativa = skillsResponse?.Expectativa ?? string.Empty,
        Numero = 10
      };

      _logger.LogInformation("Gerando questões via Dify - JobId: {JobId}", jobData.Id);
      return await _difyService.GenerateQuestionsAsync(questionsRequest);
    }
    private async Task SendEmail(ApplicationMovedPayload payload, string candidateEmail, string defaultPassword)
    {
      _logger.LogInformation("📧 Preparando envio de email - Candidato: {Email}, JobId: {JobId}, Senha: {HasPassword}", 
        candidateEmail, payload.Data.Job.Id, !string.IsNullOrEmpty(defaultPassword) ? "SIM" : "NÃO");

      // Busca a vaga no Firebase (já foi validada antes, mas precisamos do objeto)
      var job = await _firebaseService.GetJobByIdentifierAsync(_companyId, payload.Data.Job.Id.ToString());

      if (job == null)
      {
        var errorMessage = $"Vaga não encontrada no Firebase - JobId: {payload.Data.Job.Id}";
        _logger.LogError("❌ {ErrorMessage}", errorMessage);
        throw new InvalidOperationException(errorMessage);
      }

      var linkVaga = $"https://interview.coploy.io/job/{job.Id}/company/{_companyId}/login";
      _logger.LogInformation("🔗 Link da vaga gerado: {LinkVaga} - Firebase JobId: {FirebaseJobId}", 
        linkVaga, job.Id);
      
      await _emailService.SendJobPostingEmailAsync(linkVaga, candidateEmail, defaultPassword, payload.Data.Job.Name);
      
      _logger.LogInformation("✅ Email enviado com sucesso para {Email}", candidateEmail);
    }
    private static string GenerateRandomPassword()
    {
      const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
      const string numbers = "0123456789";
      const string specialChars = "!@#$%^&*";

      var random = new Random();
      var password = new StringBuilder();

      // Ensure one of each required character type
      password.Append(upperCase[random.Next(upperCase.Length)]);  // 1 uppercase
      password.Append(lowerCase[random.Next(lowerCase.Length)]);  // 1 lowercase
      password.Append(numbers[random.Next(numbers.Length)]);      // 1 number
      password.Append(specialChars[random.Next(specialChars.Length)]); // 1 special char

      // Fill the rest randomly (4-6 more chars for total length 8-10)
      var allChars = upperCase + lowerCase + numbers + specialChars;
      var remainingLength = random.Next(4, 7);

      for (int i = 0; i < remainingLength; i++)
      {
        password.Append(allChars[random.Next(allChars.Length)]);
      }

      // Shuffle the password
      return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
    }

  }
}
