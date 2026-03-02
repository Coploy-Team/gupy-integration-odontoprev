using System.Security.Cryptography;
using System.Text;

namespace GupyIntegration.Helpers
{
  public static class ApiKeyGenerator
  {
    public static string GenerateApiKey()
    {
      var key = new byte[32];
      using (var generator = RandomNumberGenerator.Create())
      {
        generator.GetBytes(key);
        return Convert.ToBase64String(key);
      }
    }

    public static string HashApiKey(string apiKey)
    {
      using (var sha256 = SHA256.Create())
      {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashedBytes);
      }
    }
  }
}