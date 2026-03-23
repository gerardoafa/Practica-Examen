using Google.Cloud.Firestore;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public class LibrosService : ILibrosService
{
    private readonly FirebaseService _firebaseService;
    private const string CollectionName = "libros";

    public LibrosService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<Libro?> GetByIdAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var document = await collection.Document(id).GetSnapshotAsync();
        
        if (!document.Exists) return null;
        
        var libro = document.ConvertTo<Libro>();
        libro.Id = document.Id;
        return libro;
    }

    public async Task<List<Libro>> GetAllAsync()
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents.Select(doc =>
        {
            var libro = doc.ConvertTo<Libro>();
            libro.Id = doc.Id;
            return libro;
        }).ToList();
    }

    public async Task<Libro> CreateAsync(Libro libro)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document();
        libro.Id = docRef.Id;
        
        await docRef.SetAsync(libro);
        return libro;
    }

    public async Task<Libro> UpdateAsync(string id, Libro libro)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        
        await docRef.SetAsync(libro, SetOptions.Overwrite);
        libro.Id = id;
        return libro;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var collection = _firebaseService.GetCollection(CollectionName);
        var docRef = collection.Document(id);
        await docRef.DeleteAsync();
        return true;
    }
}