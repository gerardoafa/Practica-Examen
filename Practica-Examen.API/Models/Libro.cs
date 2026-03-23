using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Libro
{
    [FirestoreDocumentId]
    public string? Id { get; set; }

    [FirestoreProperty("titulo")]
    public string Titulo { get; set; } = "";

    [FirestoreProperty("autor")]
    public string Autor { get; set; } = "";

    [FirestoreProperty("isbn")]
    public string Isbn { get; set; } = "";

    [FirestoreProperty("categoria")]
    public string Categoria { get; set; } = "";

    [FirestoreProperty("editorial")]
    public string Editorial { get; set; } = "";

    [FirestoreProperty("anoPublicacion")]
    public int AnoPublicacion { get; set; }

    [FirestoreProperty("copiasDisponibles")]
    public int CopiasDisponibles { get; set; }

    [FirestoreProperty("copiasTotal")]
    public int CopiasTotal { get; set; }

    [FirestoreProperty("ubicacion")]
    public string Ubicacion { get; set; } = "";

    [FirestoreProperty("estado")]
    public string Estado { get; set; } = "disponible";

    [FirestoreProperty("descripcion")]
    public string Descripcion { get; set; } = "";

    [FirestoreProperty("fechaIngreso")]
    public Timestamp FechaIngreso { get; set; }
}