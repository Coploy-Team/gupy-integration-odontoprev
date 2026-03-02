using Google.Cloud.Firestore;

namespace GupyIntegration.Models.Firebase
{
  [FirestoreData]
  public class JobApplied
  {
    [FirestoreDocumentId]
    public string? Id { get; set; }
    [FirestoreProperty("candidateStatus")]
    public string CandidateStatus { get; set; }

    [FirestoreProperty("interview")]
    public Interview Interview { get; set; }


  }
  [FirestoreData]
  public class Interview
  {
    [FirestoreProperty("additional")]
    public List<Additional> Additional { get; set; }
  }
  [FirestoreData]
  public class Additional
  {
    [FirestoreProperty("answer")]
    public string? Answer { get; set; }
    [FirestoreProperty("question")]
    public string Question { get; set; }


  }
}