namespace GupyIntegration.Models.Response
{
  public class InterviewQuestionsResponse
  {
    public string Name { get; set; }
    public List<QuestionAnswer> Questions { get; set; }
  }

  public class QuestionAnswer
  {
    public string Question { get; set; }
    public string Answer { get; set; }
  }
}