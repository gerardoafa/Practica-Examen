using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Practica_Examen.API.Models;

[FirestoreData]
public class Usuario
{
    [FirestoreDocumentId]
    public string? Id { get; set; }

    [FirestoreProperty("nombre")]
    public string Nombre { get; set; } = "";

    [FirestoreProperty("apellido")]
    public string Apellido { get; set; } = "";

    [FirestoreProperty("correo")]
    public string Correo { get; set; } = "";

    [FirestoreProperty("contrasena")]
    public string Contrasena { get; set; } = "";

    [FirestoreProperty("edad")]
    public int Edad { get; set; }

    [FirestoreProperty("numeroIdentidad")]
    public string NumeroIdentidad { get; set; } = "";

    [FirestoreProperty("telefono")]
    public string Telefono { get; set; } = "";

    [FirestoreProperty("rol")]
    public string Rol { get; set; } = "usuario";

    [FirestoreProperty("activo")]
    public bool Activo { get; set; } = true;

    [FirestoreProperty("fechaRegistro")]
    public Timestamp FechaRegistro { get; set; }

    [FirestoreProperty("multas")]
    public double Multas { get; set; } = 0;
}