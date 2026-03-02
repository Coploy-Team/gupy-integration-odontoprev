using GupyIntegration.Models.Firebase;

public interface IFirebaseService
{
  Task SetAsync<T>(string path, T data);
  Task<T?> GetAsync<T>(string path);
  Task UpdateAsync<T>(string path, T data);
  Task DeleteAsync(string path);
  Task SaveJobPostingAsync(string companyId, PostJob jobPost);
  Task<Users?> CheckUserExistsByEmailAsync(string email);
  Task CreateUserAsync(string email, string password, string name, string externalId, string phoneNumber);
  Task UpdateUserExternalIdAsync(string path, string externalId);
  Task<PostJob?> GetJobByIdentifierAsync(string companyId, string identifier);
  Task<PostJob?> GetJobByJobIdAsync(string companyId, string jobId);
  Task<List<T>?> GetSubAsync<T>(string path);
  Task<T?> GetSubIdAsync<T>(string path);
  Task<T?> GetByFullPathAsync<T>(string fullPath);
  Task<List<T>?> GetCollectionByPathAsync<T>(string fullPath);
  Task UpdatePartialAsync(string fullPath, Dictionary<string, object> updates);
}