using Google.Cloud.Firestore;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public class PrestamosService : IPrestamosService
{
    private readonly FirebaseService _firebaseService;
    private const string CollectionName = "prestamos";

    public PrestamosService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<Prestamo?> GetByIdAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var document = await collection.Document(id).GetSnapshotAsync();
        
        if (!document.Exists) return null;
        
        var prestamo = document.ConvertTo<Prestamo>();
        prestamo.Id = document.Id;
        return prestamo;
    }

    public async Task<List<Prestamo>> GetAllAsync()
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents.Select(doc =>
        {
            var prestamo = doc.ConvertTo<Prestamo>();
            prestamo.Id = doc.Id;
            return prestamo;
        }).ToList();
    }

    public async Task<List<Prestamo>> GetByUsuarioIdAsync(string usuarioId)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var query = await collection.WhereEqualTo("usuarioId", usuarioId).GetSnapshotAsync();
        
        return query.Documents.Select(doc =>
        {
            var prestamo = doc.ConvertTo<Prestamo>();
            prestamo.Id = doc.Id;
            return prestamo;
        }).ToList();
    }

    public async Task<Prestamo> CreateAsync(Prestamo prestamo)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document();
        prestamo.Id = docRef.Id;
        prestamo.FechaPrestamo = DateTime.UtcNow;
        
        await docRef.SetAsync(prestamo);
        return prestamo;
    }

    public async Task<Prestamo> UpdateAsync(string id, Prestamo prestamo)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        
        await docRef.SetAsync(prestamo, SetOptions.Overwrite);
        prestamo.Id = id;
        return prestamo;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        await docRef.DeleteAsync();
        return true;
    }
}