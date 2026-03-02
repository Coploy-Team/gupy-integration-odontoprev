using System.Text.RegularExpressions;

namespace GupyIntegration.Services.Helpers
{
  public static class HtmlContentCleaner
  {
    public static string Clean(string html)
    {
      if (string.IsNullOrEmpty(html))
        return string.Empty;

      // Remove tags específicas mantendo quebras de linha
      html = html.Replace("<p>", "")
                .Replace("</p>", "\n")
                .Replace("<br>", "\n")
                .Replace("<strong>", "")
                .Replace("</strong>", "")
                .Replace("&nbsp;", " ")
                .Replace("&bull;", "•")
                .Replace("&middot;", "·");

      // Remove quaisquer outras tags HTML
      html = Regex.Replace(html, "<[^>]*>", "");

      // Normaliza quebras de linha múltiplas
      html = Regex.Replace(html, @"\n{3,}", "\n\n");

      return html.Trim();
    }
  }
}