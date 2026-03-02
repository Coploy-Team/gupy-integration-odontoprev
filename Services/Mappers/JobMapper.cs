using Google.Cloud.Firestore;
using GupyIntegration.Models.Firebase;
using GupyIntegration.Models.Dify;
using GupyIntegration.Services.Helpers;
using GupyIntegration.Models;

namespace GupyIntegration.Services.Mappers
{
  public static class JobMapper
  {
    public static PostJob MapToPostJob(
        Result jobData,
        string title,
        DifyJobResponse skillsResponse,
        List<string> questions)
    {
      var postJob = new PostJob
      {
        JobId = jobData.Id.ToString(),
        JobName = !string.IsNullOrEmpty(title)
              ? title
              : throw new InvalidOperationException("Título da vaga não pode ser nulo"),
        TimeCreated = Timestamp.FromDateTime(DateTime.UtcNow),
        Public = false,
        Archived = false,
        Stopped = false,
        Language = "pt",
        InfoJobsBool = false,
        CompanyName = "Odontoprev",

        Address = new Address
        {
          Country = jobData?.AddressCountry ?? string.Empty,
          City = jobData?.AddressCity ?? string.Empty,
          State = jobData?.AddressState ?? string.Empty,
        },

        JobDescription = HtmlContentCleaner.Clean(jobData?.Description ?? "") ?? "Descrição não disponível",
        JobResponsabilities = HtmlContentCleaner.Clean(jobData?.Responsibilities ?? "") ?? "Responsabilidades não disponíveis",
        JobRequirements = HtmlContentCleaner.Clean(jobData?.Prerequisites ?? "") ?? "Pré-requisitos não disponíveis",
        EmploymentType = jobData?.WorkplaceType ?? "Tipo de trabalho não especificado",
        CareerLevel = GetCareerLevel(jobData?.CustomFields ?? []),
        Identifier = jobData?.Id.ToString() ?? string.Empty,
        JobCategories = jobData?.DepartmentName ?? string.Empty,
        ClosingDate = GetClosingDateTimestamp(jobData?.ApplicationDeadline ?? string.Empty),
        TypeInterview = "interview",
        UsersApplied = [],
        EducationalRequirements = [],
        CompetenciasCriticas = skillsResponse?.CompetenciasCriticas ?? string.Empty,
        CompetenciasAdicionais = skillsResponse?.CompetenciasAdicionais ?? string.Empty,
        Expectativa = skillsResponse?.Expectativa ?? string.Empty,
        JobQuestions = MapQuestions(questions)
      };

      return postJob;
    }

    private static List<JobQuestion> MapQuestions(List<string> questions)
    {
      var jobQuestions = new List<JobQuestion>();
      if (questions != null)
      {
        foreach (var question in questions)
        {
          jobQuestions.Add(new JobQuestion
          {
            Id = Guid.NewGuid().ToString(),
            Question = question
          });
        }
      }
      return jobQuestions;
    }

    public static string GetCareerLevel(IEnumerable<dynamic> customFields)
    {
      try
      {
        if (customFields == null)
          return "Não especificado";

        var careerField = customFields.FirstOrDefault(x =>
            x != null &&
            x?.Label != null &&
            x?.Label.ToString() == "Senioridade");

        return careerField?.Value?.String ?? "Não especificado";
      }
      catch
      {
        return "Não especificado";
      }
    }

    private static Timestamp GetClosingDateTimestamp(dynamic applicationDeadline)
    {
      try
      {
        if (applicationDeadline == null)
          return GetDefaultTimestamp();

        string deadlineStr = applicationDeadline.ToString();
        if (string.IsNullOrEmpty(deadlineStr))
          return GetDefaultTimestamp();

        if (DateTime.TryParse(deadlineStr, out DateTime parsedDate))
        {
          return Timestamp.FromDateTime(
              DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
          );
        }

        return GetDefaultTimestamp();
      }
      catch
      {
        return GetDefaultTimestamp();
      }
    }

    private static Timestamp GetDefaultTimestamp()
    {
      return Timestamp.FromDateTime(
          DateTime.SpecifyKind(DateTime.UtcNow.AddMonths(1), DateTimeKind.Utc)
      );
    }

    public static bool GetIsTest(IEnumerable<dynamic> customFields)
    {
      try
      {
        if (customFields == null)
          return false;

        var careerField = customFields.FirstOrDefault(x =>
            x != null &&
            x?.Label != null &&
            x?.Label.ToString() == "Haverá teste Técnico? (Coploy)");

        return careerField?.Value?.Boolean ?? false;
      }
      catch
      {
        return false;
      }
    }
  }
}
