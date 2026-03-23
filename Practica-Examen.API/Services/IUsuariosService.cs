using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public interface IUsuariosService
{
    Task<Usuario?> GetByIdAsync(string id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<List<Usuario>> GetAllAsync();
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(string id, Usuario usuario);
    Task<bool> DeleteAsync(string id);
}
