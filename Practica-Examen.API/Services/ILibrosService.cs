using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public interface ILibrosService
{
    Task<Libro?> GetByIdAsync(string id);
    Task<List<Libro>> GetAllAsync();
    Task<Libro> CreateAsync(Libro libro);
    Task<Libro> UpdateAsync(string id, Libro libro);
    Task<bool> DeleteAsync(string id);
}
