using Google.Cloud.Firestore;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public class UsuariosService : IUsuariosService
{
    private readonly FirebaseService _firebaseService;
    private const string CollectionName = "usuarios";

    public UsuariosService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<Usuario?> GetByIdAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var document = await collection.Document(id).GetSnapshotAsync();
        
        if (!document.Exists) return null;
        
        var usuario = document.ConvertTo<Usuario>();
        usuario.Id = document.Id;
        return usuario;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var query = await collection.WhereEqualTo("email", email).GetSnapshotAsync();
        
        if (query.Documents.Count == 0) return null;
        
        var document = query.Documents.First();
        var usuario = document.ConvertTo<Usuario>();
        usuario.Id = document.Id;
        return usuario;
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents.Select(doc =>
        {
            var usuario = doc.ConvertTo<Usuario>();
            usuario.Id = doc.Id;
            return usuario;
        }).ToList();
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document();
        usuario.Id = docRef.Id;
        usuario.FechaRegistro = DateTime.UtcNow;
        
        await docRef.SetAsync(usuario);
        return usuario;
    }

    public async Task<Usuario> UpdateAsync(string id, Usuario usuario)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        
        await docRef.SetAsync(usuario, SetOptions.Overwrite);
        usuario.Id = id;
        return usuario;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        await docRef.DeleteAsync();
        return true;
    }
}