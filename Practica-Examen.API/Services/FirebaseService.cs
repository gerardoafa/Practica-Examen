using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Newtonsoft.Json;

namespace Practica_Examen.API.Services;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirebaseService> _logger;

    public FirebaseService(ILogger<FirebaseService> logger)
    {
        _logger = logger;

        try
        {
            var credentialsPath = Path.Combine(
                AppContext.BaseDirectory,
                "Config",
                "practica-examen.json"
            );

            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    $"Archivo de credenciales no encontrado en: {credentialsPath}"
                );
            }

            var projectId = GetProjectIdFromCredentials(credentialsPath);

            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                credentialsPath
            );

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(
                    new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    }
                );
            }

            var firebaseClientBuilder = new FirestoreClientBuilder
            {
                ChannelCredentials = GoogleCredential.FromFile(credentialsPath)
                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform")
                    .ToChannelCredentials()
            };

            var firestoreClient = firebaseClientBuilder.Build();
            _firestoreDb = FirestoreDb.Create(projectId, firestoreClient);
            Console.WriteLine("Conexión a Firebase iniciada correctamente");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al iniciar Firebase: {e.Message}");
            Console.WriteLine($"Stack trace: {e.StackTrace}");
            throw;
        }
    }

    private string GetProjectIdFromCredentials(string credentialsPath)
    {
        var json = File.ReadAllText(credentialsPath);
        dynamic credentials = JsonConvert.DeserializeObject(json);
        return credentials["project_id"];
    }

    public CollectionReference GetCollection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
}