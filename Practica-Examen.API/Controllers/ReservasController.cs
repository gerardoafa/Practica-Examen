using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica_Examen.API.Data;
using Practica_Examen.API.Models;
using System.Security.Claims;

namespace Practica_Examen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST /api/reservas
        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] CrearReservaDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var libro = await _context.Libros.FindAsync(dto.LibroId);

            if (libro == null || libro.CopiasDisponibles != 0)
                return BadRequest("El libro debe existir y no tener copias disponibles.");

            var yaTieneReserva = await _context.Reservas.AnyAsync(r =>
                r.UsuarioId == usuarioId &&
                r.LibroId == dto.LibroId &&
                (r.Estado == "pendiente" || r.Estado == "notificada"));

            if (yaTieneReserva)
                return BadRequest("Ya tiene una reserva para este libro.");

            var totalPendientes = await _context.Reservas
                .CountAsync(r => r.LibroId == dto.LibroId && r.Estado == "pendiente");

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                LibroId = dto.LibroId,
                Prioridad = totalPendientes + 1,
                Estado = "pendiente"
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CrearReserva), new { id = reserva.Id }, reserva);
        }

        // GET /api/reservas/mis-reservas
        [HttpGet("mis-reservas")]
        public async Task<IActionResult> MisReservas()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var reservas = await _context.Reservas
                .Where(r => r.UsuarioId == usuarioId)
                .Include(r => r.Libro)
                .OrderBy(r => r.FechaCreacion)
                .Select(r => new
                {
                    r.Id,
                    Libro = new { r.Libro.Titulo, r.Libro.Autor }, // ajusta según tus campos
                    r.Estado,
                    r.Prioridad,
                    PosicionEnCola = r.Prioridad,
                    r.FechaCreacion,
                    r.FechaNotificacion,
                    r.FechaExpiracion
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // DELETE /api/reservas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

            if (reserva == null || (reserva.Estado != "pendiente" && reserva.Estado != "notificada"))
                return BadRequest("Reserva no encontrada o no se puede cancelar.");

            var libroId = reserva.LibroId;
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            // Recalcular prioridades
            var reservasRestantes = await _context.Reservas
                .Where(r => r.LibroId == libroId && r.Estado == "pendiente")
                .OrderBy(r => r.Prioridad)
                .ToListAsync();

            for (int i = 0; i < reservasRestantes.Count; i++)
            {
                reservasRestantes[i].Prioridad = i + 1;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva cancelada y prioridades actualizadas." });
        }
    }

    public class CrearReservaDto
    {
        public int LibroId { get; set; }
    }
}
