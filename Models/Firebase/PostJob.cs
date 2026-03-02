using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class PostJob
  {

    [FirestoreDocumentId]
    public string? Id { get; set; }

    [FirestoreProperty("address")]
    public Address Address { get; set; } = new();

    [FirestoreProperty("archived")]
    public bool Archived { get; set; }

    [FirestoreProperty("carrerLevel")]
    public string CareerLevel { get; set; } = string.Empty;

    [FirestoreProperty("closingDate")]
    public Timestamp ClosingDate { get; set; }

    [FirestoreProperty("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [FirestoreProperty("educationalRequiements")]
    public List<string> EducationalRequirements { get; set; } = [];

    [FirestoreProperty("employmentType")]
    public string EmploymentType { get; set; } = string.Empty;

    [FirestoreProperty("identifier")]
    public string Identifier { get; set; } = string.Empty;

    [FirestoreProperty("infoJobsBool")]
    public bool InfoJobsBool { get; set; }

    [FirestoreProperty("jobCategories")]
    public string JobCategories { get; set; } = string.Empty;

    [FirestoreProperty("jobDescription")]
    public string JobDescription { get; set; } = string.Empty;

    [FirestoreProperty("language")]
    public string Language { get; set; } = string.Empty;

    [FirestoreProperty("jobRequirements")]
    public string JobRequirements { get; set; } = string.Empty;

    [FirestoreProperty("jobHours")]
    public string JobHours { get; set; } = string.Empty;

    [FirestoreProperty("jobId")]
    public string JobId { get; set; } = string.Empty;

    [FirestoreProperty("jobModel")]
    public string JobModel { get; set; } = string.Empty;

    [FirestoreProperty("jobName")]
    public string JobName { get; set; } = string.Empty;

    [FirestoreProperty("jobQuestions")]
    public List<JobQuestion> JobQuestions { get; set; } = [];

    [FirestoreProperty("jobResponsabilities")]
    public string JobResponsabilities { get; set; } = string.Empty;

    [FirestoreProperty("limitNumberJobVacancies")]
    public string LimitNumberJobVacancies { get; set; } = string.Empty;

    [FirestoreProperty("limitedJobVacancy")]
    public bool LimitedJobVacancy { get; set; }

    [FirestoreProperty("public")]
    public bool Public { get; set; }

    [FirestoreProperty("stopped")]
    public bool Stopped { get; set; }

    [FirestoreProperty("timeCreated")]
    public Timestamp TimeCreated { get; set; } = new();

    [FirestoreProperty("typeInterview")]
    public string TypeInterview { get; set; } = string.Empty;

    [FirestoreProperty("uid")]
    public DocumentReference? Uid { get; set; }

    [FirestoreProperty("usersApplied")]
    public List<DocumentReference> UsersApplied { get; set; } = [];

    [FirestoreProperty("competenciasCriticas")]
    public string CompetenciasCriticas { get; set; } = string.Empty;

    [FirestoreProperty("competenciasAdicionais")]
    public string CompetenciasAdicionais { get; set; } = string.Empty;

    [FirestoreProperty("expectativa")]
    public string Expectativa { get; set; } = string.Empty;

    [FirestoreProperty("companyInterviews")]
    public List<CompanyInterviews> CompanyInterviews { get; set; } = [];

  }
}