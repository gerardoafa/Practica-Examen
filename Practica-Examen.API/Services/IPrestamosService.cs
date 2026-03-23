using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public interface IPrestamosService
{
    Task<Prestamo?> GetByIdAsync(string id);
    Task<List<Prestamo>> GetAllAsync();
    Task<List<Prestamo>> GetByUsuarioIdAsync(string usuarioId);
    Task<Prestamo> CreateAsync(Prestamo prestamo);
    Task<Prestamo> UpdateAsync(string id, Prestamo prestamo);
    Task<bool> DeleteAsync(string id);
}
