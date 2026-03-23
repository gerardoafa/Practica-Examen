using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Prestamo
{
    [JsonPropertyName("id")]
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("usuarioId")]
    [FirestoreProperty]
    public string UsuarioId { get; set; } = string.Empty;

    [JsonPropertyName("libroId")]
    [FirestoreProperty]
    public string LibroId { get; set; } = string.Empty;

    [JsonPropertyName("fechaPrestamo")]
    [FirestoreProperty]
    public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("fechaDevolucion")]
    [FirestoreProperty]
    public DateTime? FechaDevolucion { get; set; }

    [JsonPropertyName("estado")]
    [FirestoreProperty]
    public string Estado { get; set; } = "activo";
}