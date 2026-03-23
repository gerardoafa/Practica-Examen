using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Usuario
{
        [JsonPropertyName("id")]
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        [FirestoreProperty]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        [FirestoreProperty]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("rol")]
        [FirestoreProperty]
        public string Rol { get; set; } = "usuario";

        [JsonPropertyName("fechaRegistro")]
        [FirestoreProperty]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }