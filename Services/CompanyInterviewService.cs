using GupyIntegration.Models.Firebase;
using GupyIntegration.Models.Response;
using GupyIntegration.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Google.Cloud.Firestore;
using GupyIntegration.Models;

namespace GupyIntegration.Services
{
  public class CompanyInterviewService : ICompanyInterviewService
  {
    private readonly IFirebaseService _firebaseService;
    private readonly string _companyId;
    private readonly ILogger<CompanyInterviewService> _logger;
    private readonly IGupyApplicationService _gupyApplicationService;


    public CompanyInterviewService(
        IFirebaseService firebaseService,
        IConfiguration configuration,
        ILogger<CompanyInterviewService> logger,
        IGupyApplicationService gupyApplicationService,
        IEmailService emailService)
    {
      _firebaseService = firebaseService;
      _logger = logger;
      _companyId = configuration.GetValue<string>("Company:DefaultCompanyId")
          ?? throw new InvalidOperationException("DefaultCompanyId not configured");
      _gupyApplicationService = gupyApplicationService;
    }

    private CompanyInterviewResponse MapToResponse(CompanyInterviews interview, string? jobIdentifier = null)
    {
      return new CompanyInterviewResponse
      {
        CandidateStatus = interview.CandidateStatus,
        CareerLevel = interview.CareerLevel,
        City = interview.City,
        Date = interview.Date,
        DateSelect = interview.DateSelect,
        Email = interview.Email,
        ExternalId = interview.ExternalId,
        Finish = interview.Finish,
        JobDescription = interview.JobDescription,
        JobName = interview.JobName,
        Name = interview.Name,
        Occupation = interview.Occupation,
        PhoneNumber = interview.PhoneNumber,
        PhotoUrl = interview.PhotoUrl,
        ProfessionalExperience = interview.ProfessionalExperience,
        Score = interview.Score,
        State = interview.State,
        Stopped = interview.Stopped,
        TypeInterview = interview.TypeInterview,
        JobAppliedRefId = interview.JobAppliedRefId.Id,
        JobRefId = interview.JobRefId.Id,
        UserRefId = interview.UserRefId.Id,
        JobIdentifier = jobIdentifier
      };
    }

    public async Task<List<CompanyInterviewResponse>> GetCompanyInterviewsAsync()
    {
      try
      {
        _logger.LogInformation("Buscando entrevistas da empresa {CompanyId}", _companyId);

        var path = $"companies/{_companyId}/companyInterviews";
        var interviews = await _firebaseService.GetSubAsync<CompanyInterviews>(path);

        var ninetyDaysAgo = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-9));
        var filteredInterviews = interviews?.Where(x => x.Finish == true && x.Date >= ninetyDaysAgo).ToList()
            ?? new List<CompanyInterviews>();

        var responseList = new List<CompanyInterviewResponse>();

        foreach (var interview in filteredInterviews)
        {
          var jobPath = $"companies/{_companyId}/postJob/{interview.JobRefId.Id}";
          var postJob = await _firebaseService.GetAsync<PostJob>(jobPath);
          var jobIdentifier = postJob?.Identifier;

          responseList.Add(MapToResponse(interview, jobIdentifier));
        }

        _logger.LogInformation("Encontradas {Count} entrevistas", responseList.Count);
        return responseList;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar entrevistas da empresa {CompanyId}", _companyId);
        throw;
      }
    }

    public async Task<CompanyInterviewResponse?> GetCompanyInterviewByIdAsync(string interviewId)
    {
      try
      {
        _logger.LogInformation("Buscando entrevista {InterviewId} da empresa {CompanyId}",
            interviewId, _companyId);

        var path = $"companies/{_companyId}/companyInterviews/{interviewId}";
        var interview = await _firebaseService.GetAsync<CompanyInterviews>(path);

        if (interview == null)
        {
          _logger.LogWarning("Entrevista {InterviewId} não encontrada", interviewId);
          return null;
        }

        var jobPath = $"companies/{_companyId}/postJob/{interview.JobRefId.Id}";
        var postJob = await _firebaseService.GetAsync<PostJob>(jobPath);
        var jobIdentifier = postJob?.Identifier;

        return MapToResponse(interview, jobIdentifier);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar entrevista {InterviewId} da empresa {CompanyId}",
            interviewId, _companyId);
        throw;
      }
    }

    public async Task<List<string>> GetSimplifiedInterviewsAsync()
    {
      try
      {
        _logger.LogInformation("Buscando lista simplificada de entrevistas da empresa {CompanyId}", _companyId);

        var path = $"companies/{_companyId}/companyInterviews";
        var interviews = await _firebaseService.GetSubAsync<CompanyInterviews>(path);

        var ninetyDaysAgo = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-9));
        var filteredInterviews = interviews?.Where(x => x.Finish == true && x.Date >= ninetyDaysAgo && !string.IsNullOrEmpty(x.ExternalId)).ToList()
            ?? new List<CompanyInterviews>();


        var responseList = new List<CompanyInterviewSimplifiedResponse>();

        foreach (var interview in filteredInterviews)
        {
          var jobPath = $"companies/{_companyId}/postJob/{interview.JobRefId.Id}";
          var postJob = await _firebaseService.GetSubIdAsync<PostJob>(jobPath);

          if (postJob?.Identifier != null)
          {
            responseList.Add(new CompanyInterviewSimplifiedResponse
            {
              Id = interview.UserRefId.Id,
              Email = interview.Email,
              ExternalId = interview.ExternalId,
              JobIdentifier = postJob.Identifier,
              Score = interview.Score
            });
          }
        }
        var indetifier = responseList.Select(x => x.JobIdentifier).Distinct().ToList();

        foreach (var identifier in indetifier)
        {
          if (!int.TryParse(identifier, out int jobId))
          {
            _logger.LogWarning("Invalid job identifier format: {Identifier}", identifier);
            continue;
          }

          var allApplications = new List<ApplicationResult>();
          var totalPages = await _gupyApplicationService.GetApplicationsAsync(jobId, page: 1, perPage: 70);
          for (int page = 1; page <= totalPages.TotalPages; page++)
          {
            var applications = await _gupyApplicationService.GetApplicationsAsync(jobId, page: page, perPage: 70);
            if (applications?.Results != null)
            {
              allApplications.AddRange(applications.Results);
            }
          }
          _logger.LogInformation("Encontradas {Count} aplicacoes", allApplications.Count);

          var applicationsList = allApplications ?? new();

          foreach (var user in responseList.Where(x => x.JobIdentifier == identifier))
          {
            var candidate = applicationsList.FirstOrDefault(x => x.Candidate.Email == user.Email);

            if (candidate != null)
            {
              await _firebaseService.UpdateUserExternalIdAsync($"users/{user.Id}", candidate?.Id.ToString() ?? "0");
              try
              {
                await _gupyApplicationService.AddTagToApplicationAsync(jobId, candidate.Id, new ApplicationTag
                {
                  Name = user.Score.ToString().Replace(",", "."),
                });
              }
              catch (System.Exception)
              {
                _logger.LogWarning("Erro ao adicionar tag de score para aplicacao {ApplicationId}", candidate.Id);
              }
            }

            _logger.LogInformation("Aplicacao {ApplicationId} encontrada", user.Id);
          }
        }

        _logger.LogInformation("Encontradas {Count} entrevistas simplificadas", responseList.Count);
        return indetifier;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar lista simplificada de entrevistas da empresa {CompanyId}", _companyId);
        throw;
      }
    }
    public async Task<List<InterviewQuestionsResponse>> GetQuestionsAddicionalAsync()
    {
      try
      {
        var result = new List<InterviewQuestionsResponse>();
        var path = $"companies/tpRjtJnp0TLjmgj6BCnL/postJob";
        var postJob = await _firebaseService.GetSubAsync<PostJob>(path);

        foreach (var job in postJob)
        {
          var pathJob = $"companies/tpRjtJnp0TLjmgj6BCnL/postJob/{job.Id}/interviews";
          var interviews = await _firebaseService.GetCollectionByPathAsync<Interviews>(pathJob);

          foreach (var interview in interviews.Where(x => x.Finished == true))
          {
            var jobApplied = await _firebaseService.GetByFullPathAsync<JobApplied>(interview?.JobApplied?.Path.Replace("projects/coployf/databases/(default)/documents/", string.Empty));
            var additional = jobApplied?.Interview?.Additional;

            if (additional != null)
            {
              result.Add(new InterviewQuestionsResponse
              {
                Name = interview.Name ?? "Unknown",
                Questions = additional.Select(item => new QuestionAnswer
                {
                  Question = item.Question,
                  Answer = item.Answer
                }).ToList()
              });
            }
          }
        }

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting additional questions");
        throw;
      }
    }
  }
}