using Google.Cloud.Firestore;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public class ReservasService : IReservasService
{
    private readonly FirebaseService _firebaseService;
    private const string CollectionName = "reservas";

    public ReservasService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<Reserva?> GetByIdAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var document = await collection.Document(id).GetSnapshotAsync();
        
        if (!document.Exists) return null;
        
        var reserva = document.ConvertTo<Reserva>();
        reserva.Id = document.Id;
        return reserva;
    }

    public async Task<List<Reserva>> GetAllAsync()
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents.Select(doc =>
        {
            var reserva = doc.ConvertTo<Reserva>();
            reserva.Id = doc.Id;
            return reserva;
        }).ToList();
    }

    public async Task<List<Reserva>> GetByUsuarioIdAsync(string usuarioId)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var query = await collection.WhereEqualTo("usuarioId", usuarioId).GetSnapshotAsync();
        
        return query.Documents.Select(doc =>
        {
            var reserva = doc.ConvertTo<Reserva>();
            reserva.Id = doc.Id;
            return reserva;
        }).ToList();
    }

    public async Task<Reserva> CreateAsync(Reserva reserva)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document();
        reserva.Id = docRef.Id;
        reserva.FechaReserva = DateTime.UtcNow;
        
        await docRef.SetAsync(reserva);
        return reserva;
    }

    public async Task<Reserva> UpdateAsync(string id, Reserva reserva)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        
        await docRef.SetAsync(reserva, SetOptions.Overwrite);
        reserva.Id = id;
        return reserva;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        await docRef.DeleteAsync();
        return true;
    }
}