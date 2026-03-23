using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica_Examen.API.Data;           // tu DbContext namespace
using Practica_Examen.API.Models;
using System.Security.Claims;

namespace Practica_Examen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   // requiere autenticación JWT
    public class PrestamosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrestamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST /api/prestamos
        [HttpPost]
        public async Task<IActionResult> CrearPrestamo([FromBody] CrearPrestamoDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            var libro = await _context.Libros.FindAsync(dto.LibroId);

            if (libro == null || libro.CopiasDisponibles <= 0)
                return BadRequest("No hay copias disponibles del libro.");

            if (usuario.Multas > 500)
                return BadRequest("Tiene multas pendientes mayores a 500 Lempiras.");

            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.UsuarioId == usuarioId && p.Estado == "activo");

            if (prestamosActivos >= 3)
                return BadRequest("No puede tener más de 3 préstamos activos.");

            var fechaPrestamo = DateTime.UtcNow;
            var fechaDevolucionEsperada = fechaPrestamo.AddDays(14);

            var prestamo = new Prestamo
            {
                UsuarioId = usuarioId,
                LibroId = dto.LibroId,
                FechaPrestamo = fechaPrestamo,
                FechaDevolucionEsperada = fechaDevolucionEsperada,
                Estado = "activo"
            };

            _context.Prestamos.Add(prestamo);
            libro.CopiasDisponibles--;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CrearPrestamo), new { id = prestamo.Id }, prestamo);
        }

        // PUT /api/prestamos/{id}/devolver
        [HttpPut("{id}/devolver")]
        public async Task<IActionResult> DevolverPrestamo(int id)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            if (prestamo == null || prestamo.Estado != "activo")
                return BadRequest("Préstamo no encontrado o ya devuelto.");

            var fechaDevolucionReal = DateTime.UtcNow;
            var diasRetraso = (int)Math.Max(0, (fechaDevolucionReal - prestamo.FechaDevolucionEsperada).TotalDays);

            var multaGenerada = diasRetraso * 50m;

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            usuario.Multas += multaGenerada;

            prestamo.FechaDevolucionReal = fechaDevolucionReal;
            prestamo.Estado = "devuelto";

            prestamo.Libro.CopiasDisponibles++;

            // Notificar primera reserva pendiente
            var primeraReserva = await _context.Reservas
                .Where(r => r.LibroId == prestamo.LibroId && r.Estado == "pendiente")
                .OrderBy(r => r.Prioridad)
                .FirstOrDefaultAsync();

            if (primeraReserva != null)
            {
                primeraReserva.Estado = "notificada";
                primeraReserva.FechaNotificacion = DateTime.UtcNow;
                primeraReserva.FechaExpiracion = DateTime.UtcNow.AddDays(7);
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Libro devuelto correctamente", multaGenerada, diasRetraso });
        }
    }

    // DTO simple
    public class CrearPrestamoDto
    {
        public int LibroId { get; set; }
    }
}
