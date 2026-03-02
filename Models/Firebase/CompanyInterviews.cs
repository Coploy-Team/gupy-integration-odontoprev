using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class CompanyInterviews
  {
    [FirestoreProperty("candidate_status")]
    public string CandidateStatus { get; set; } = string.Empty;

    [FirestoreProperty("career_level")]
    public string CareerLevel { get; set; } = string.Empty;

    [FirestoreProperty("city")]
    public string City { get; set; } = string.Empty;

    [FirestoreProperty("date")]
    public Timestamp Date { get; set; }

    [FirestoreProperty("date_select")]
    public Timestamp DateSelect { get; set; }

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("external_id")]
    public string ExternalId { get; set; } = string.Empty;

    [FirestoreProperty("finished")]
    public bool Finish { get; set; }

    [FirestoreProperty("job_applied_ref")]
    public DocumentReference JobAppliedRefId { get; set; }

    [FirestoreProperty("job_description")]
    public string JobDescription { get; set; } = string.Empty;

    [FirestoreProperty("job_name")]
    public string JobName { get; set; } = string.Empty;

    [FirestoreProperty("job_ref")]
    public DocumentReference JobRefId { get; set; }

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("occupation")]
    public string Occupation { get; set; } = string.Empty;

    [FirestoreProperty("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [FirestoreProperty("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    [FirestoreProperty("professional_experience")]
    public string ProfessionalExperience { get; set; } = string.Empty;

    [FirestoreProperty("score")]
    public double Score { get; set; }

    [FirestoreProperty("state")]
    public string State { get; set; } = string.Empty;

    [FirestoreProperty("stopped")]
    public bool Stopped { get; set; }

    [FirestoreProperty("type_interview")]
    public string TypeInterview { get; set; } = string.Empty;

    [FirestoreProperty("user_ref")]
    public DocumentReference UserRefId { get; set; }
  }
}