using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class JobQuestion
  {
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("question")]
    public string Question { get; set; } = string.Empty;
  }
}