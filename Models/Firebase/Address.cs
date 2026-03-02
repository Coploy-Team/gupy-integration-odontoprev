using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class Address
  {
    [FirestoreProperty("city")]
    public string City { get; set; } = string.Empty;

    [FirestoreProperty("country")]
    public string Country { get; set; } = string.Empty;

    [FirestoreProperty("state")]
    public string State { get; set; } = string.Empty;
  }
}