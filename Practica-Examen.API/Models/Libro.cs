using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Libro
{
    [JsonPropertyName("id")]
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("titulo")]
    [FirestoreProperty]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("autor")]
    [FirestoreProperty]
    public string Autor { get; set; } = string.Empty;

    [JsonPropertyName("isbn")]
    [FirestoreProperty]
    public string ISBN { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    [FirestoreProperty]
    public string Categoria { get; set; } = string.Empty;

    [JsonPropertyName("stock")]
    [FirestoreProperty]
    public int Stock { get; set; }

    [JsonPropertyName("disponible")]
    [FirestoreProperty]
    public bool Disponible { get; set; } = true;
}