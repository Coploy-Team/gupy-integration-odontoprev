using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class Interviews
  {
    [FirestoreDocumentId]
    public string? Id { get; set; }
    [FirestoreProperty("finished")]
    public bool Finished { get; set; }
    [FirestoreProperty("job_applied_ref")]
    public DocumentReference? JobApplied { get; set; }
    [FirestoreProperty("name")]
    public string Name { get; set; }
    [FirestoreProperty("user_ref")]
    public DocumentReference? UserRef { get; set; }
  }
}