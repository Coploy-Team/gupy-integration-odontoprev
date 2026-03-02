using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using GupyIntegration.Models.Configurations;
using GupyIntegration.Models.Firebase;

namespace GupyIntegration.Services
{
  public class FirebaseService : IFirebaseService
  {
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirebaseService> _logger;

    public FirebaseService(IOptions<FirebaseConfig> config, ILogger<FirebaseService> logger)
    {
      if (FirebaseApp.DefaultInstance == null)
      {
        var credentials = GoogleCredential.FromJson(@"{
                    ""type"": ""service_account"",
                    ""project_id"": """ + config.Value.ProjectId + @""",
                    ""private_key"": """ + config.Value.PrivateKey + @""",
                    ""client_email"": """ + config.Value.ClientEmail + @"""
                }");

        FirebaseApp.Create(new AppOptions
        {
          Credential = credentials,
          ProjectId = config.Value.ProjectId
        });
      }

      _firestoreDb = FirestoreDb.Create(config.Value.ProjectId);
      _logger = logger;
    }

    public async Task SetAsync<T>(string path, T data)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
      await docRef.SetAsync(data);
    }
    public async Task UpdateUserExternalIdAsync(string path, string externalId)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
      await docRef.UpdateAsync(new Dictionary<string, object>
      {
       { "external_id", externalId}
      });
    }

    public async Task<T?> GetAsync<T>(string path)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
      var snapshot = await docRef.GetSnapshotAsync();

      if (snapshot.Exists)
      {
        return snapshot.ConvertTo<T>();
      }

      return default;
    }
    public async Task<List<T>?> GetSubAsync<T>(string path)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];
      var subCollectionName = pathParts[2];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId).Collection(subCollectionName);
      var snapshot = await docRef.GetSnapshotAsync();

      if (snapshot.Documents.Count > 0)
      {
        return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
      }

      return default;
    }
    public async Task<T?> GetSubIdAsync<T>(string path)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];
      var subCollectionName = pathParts[2];
      var docId = pathParts[3];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId).Collection(subCollectionName).Document(docId);
      var snapshot = await docRef.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        return snapshot.ConvertTo<T>();
      }

      return default;
    }

    public async Task UpdateAsync<T>(string path, T data)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
      await docRef.UpdateAsync(data as IDictionary<string, object>);
    }

    public async Task DeleteAsync(string path)
    {
      var pathParts = path.Split('/');
      var collectionName = pathParts[0];
      var documentId = pathParts[1];

      var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
      await docRef.DeleteAsync();
    }

    public async Task SaveJobPostingAsync(string companyId, PostJob jobPost)
    {
      try
      {
        var collectionReference = _firestoreDb
          .Collection("companies")
          .Document(companyId)
          .Collection("postJob");

        // Add the document and get its reference
        var docRef = await collectionReference.AddAsync(jobPost);

        // Update the PostJob object with the generated ID
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
          { "uid", docRef }
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao salvar vaga no Firebase: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<Users?> CheckUserExistsByEmailAsync(string email)
    {
      try
      {
        var usersRef = _firestoreDb.Collection("users");
        var query = usersRef.WhereEqualTo("email", email);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<Users>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao verificar usuário: {ex.Message}");
        throw;
      }
    }

    public async Task CreateUserAsync(string email, string password, string name, string externalId, string phoneNumber)
    {
      try
      {
        _logger.LogDebug("Iniciando criação de usuário no Firebase Auth - Email: {Email}", email);
        var photoUrl = $"https://api.dicebear.com/8.x/initials/png?backgroundColor=ffb4e7,ffea83,80e5f4,cdf784&fontFamily=sans-serif&fontSize=36&fontWeight=100&seed={name}";
        // Criar usuário no Firebase Auth
        var auth = FirebaseAuth.DefaultInstance;
        var userArgs = new UserRecordArgs
        {
          Email = email,
          Password = password,
          DisplayName = name,
          EmailVerified = true,
        };

        var userRecord = await auth.CreateUserAsync(userArgs);
        _logger.LogDebug("Usuário criado com sucesso no Firebase Auth - UID: {Uid}", userRecord.Uid);

        // Criar documento correspondente no Firestore
        var userData = new Dictionary<string, object>
        {
          { "display_name", name },
          { "email", email },
          { "external_id", externalId },
          { "created_time", Timestamp.FromDateTime(DateTime.UtcNow) },
          { "photo_url", photoUrl },
          { "phone_number", phoneNumber }
        };

        // Usar o UID do Auth como ID do documento no Firestore
        await _firestoreDb.Collection("users").Document(userRecord.Uid).SetAsync(userData);

        _logger.LogInformation("Usuário criado com sucesso no Firebase Auth e Firestore - Email: {Email}, UID: {Uid}",
            email, userRecord.Uid);
      }
      catch (FirebaseAuthException authEx)
      {
        _logger.LogError(authEx, "Erro ao criar usuário no Firebase Auth - Email: {Email}, Código: {ErrorCode}",
            email, authEx.ErrorCode);
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao criar usuário no Firebase - Email: {Email}", email);
        throw;
      }
    }

    public async Task<PostJob?> GetJobByIdentifierAsync(string companyId, string identifier)
    {
      try
      {
        var jobsQuery = _firestoreDb.Collection("companies").Document(companyId).Collection("postJob")
          .WhereEqualTo("identifier", identifier);

        var snapshot = await jobsQuery.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<PostJob>();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching job post by identifier: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<PostJob?> GetJobByJobIdAsync(string companyId, string jobId)
    {
      try
      {
        var jobsQuery = _firestoreDb.Collection("companies").Document(companyId).Collection("postJob")
          .WhereEqualTo("jobId", jobId);

        var snapshot = await jobsQuery.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<PostJob>();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching job post by jobId: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<T?> GetByFullPathAsync<T>(string fullPath)
    {
      try
      {
        var pathParts = fullPath.Split('/');
        var documentRef = _firestoreDb.Document(fullPath);
        var snapshot = await documentRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
          return snapshot.ConvertTo<T>();
        }

        return default;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting document by path: {Path}", fullPath);
        throw;
      }
    }

    public async Task<List<T>?> GetCollectionByPathAsync<T>(string fullPath)
    {
      try
      {
        var collectionRef = _firestoreDb.Collection(fullPath);
        var snapshot = await collectionRef.GetSnapshotAsync();

        if (!snapshot.Any())
        {
          return new List<T>();
        }

        return snapshot.Documents
            .Where(doc => doc.Exists)
            .Select(doc => doc.ConvertTo<T>())
            .ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting collection by path: {Path}", fullPath);
        throw;
      }
    }

    public async Task UpdatePartialAsync(string fullPath, Dictionary<string, object> updates)
    {
      try
      {
        var documentRef = _firestoreDb.Document(fullPath);
        await documentRef.UpdateAsync(updates);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating document at path: {Path}", fullPath);
        throw;
      }
    }
  }
}