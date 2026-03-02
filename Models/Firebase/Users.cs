using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class Users
  {
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("created_time")]
    public Timestamp CreatedTime { get; set; }

    [FirestoreProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("external_id")]
    public string? ExternalId { get; set; }

    [FirestoreProperty("jobsApplied")]
    public List<string>? JobsApplied { get; set; }

    [FirestoreProperty("occupation")]
    public string? Occupation { get; set; }

    [FirestoreProperty("pdf_socioEmotional")]
    public string? PdfSocioEmotional { get; set; }

    [FirestoreProperty("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [FirestoreProperty("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;
  }
}