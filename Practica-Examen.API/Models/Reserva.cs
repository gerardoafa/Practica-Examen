using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Reserva
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

    [JsonPropertyName("fechaReserva")]
    [FirestoreProperty]
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("estado")]
    [FirestoreProperty]
    public string Estado { get; set; } = "activa";
}