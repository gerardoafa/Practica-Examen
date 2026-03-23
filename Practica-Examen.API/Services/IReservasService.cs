using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Services;

public interface IReservasService
{
    Task<Reserva?> GetByIdAsync(string id);
    Task<List<Reserva>> GetAllAsync();
    Task<List<Reserva>> GetByUsuarioIdAsync(string usuarioId);
    Task<Reserva> CreateAsync(Reserva reserva);
    Task<Reserva> UpdateAsync(string id, Reserva reserva);
    Task<bool> DeleteAsync(string id);
}
