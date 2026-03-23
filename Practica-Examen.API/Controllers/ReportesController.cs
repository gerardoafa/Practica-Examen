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
    [Authorize]  // Solo usuarios autenticados (puedes cambiar a [Authorize(Roles = "Admin")] si es solo para administrador)
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reportes/prestamos-activos
        [HttpGet("prestamos-activos")]
        public async Task<IActionResult> PrestamosActivos()
        {
            var prestamos = await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                .Where(p => p.Estado == "activo")
                .Select(p => new
                {
                    p.Id,
                    Usuario = p.Usuario.Nombre + " " + p.Usuario.Apellido, // ajusta según tus campos
                    Libro = p.Libro.Titulo,
                    p.FechaPrestamo,
                    p.FechaDevolucionEsperada,
                    DiasRestantes = (p.FechaDevolucionEsperada - DateTime.UtcNow).Days
                })
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();

            return Ok(prestamos);
        }

        // GET: api/reportes/prestamos-vencidos
        [HttpGet("prestamos-vencidos")]
        public async Task<IActionResult> PrestamosVencidos()
        {
            var hoy = DateTime.UtcNow;

            var vencidos = await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Libro)
                .Where(p => p.Estado == "activo" && p.FechaDevolucionEsperada < hoy)
                .Select(p => new
                {
                    p.Id,
                    Usuario = p.Usuario.Nombre + " " + p.Usuario.Apellido,
                    Libro = p.Libro.Titulo,
                    p.FechaPrestamo,
                    p.FechaDevolucionEsperada,
                    DiasRetraso = (hoy - p.FechaDevolucionEsperada).Days,
                    MultaEstimada = (hoy - p.FechaDevolucionEsperada).Days * 50m
                })
                .OrderByDescending(p => p.DiasRetraso)
                .ToListAsync();

            return Ok(vencidos);
        }

        // GET: api/reportes/multas-usuarios
        [HttpGet("multas-usuarios")]
        public async Task<IActionResult> MultasPorUsuario()
        {
            var usuariosConMultas = await _context.Usuarios
                .Where(u => u.Multas > 0)
                .Select(u => new
                {
                    u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    u.Multas,
                    PrestamosActivos = _context.Prestamos.Count(p => p.UsuarioId == u.Id && p.Estado == "activo")
                })
                .OrderByDescending(u => u.Multas)
                .ToListAsync();

            return Ok(usuariosConMultas);
        }

        // GET: api/reportes/reservas-pendientes
        [HttpGet("reservas-pendientes")]
        public async Task<IActionResult> ReservasPendientes()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Libro)
                .Where(r => r.Estado == "pendiente")
                .Select(r => new
                {
                    r.Id,
                    Usuario = r.Usuario.Nombre + " " + r.Usuario.Apellido,
                    Libro = r.Libro.Titulo,
                    r.Prioridad,
                    r.FechaCreacion
                })
                .OrderBy(r => r.FechaCreacion)
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/reportes/reservas-notificadas
        [HttpGet("reservas-notificadas")]
        public async Task<IActionResult> ReservasNotificadas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Libro)
                .Where(r => r.Estado == "notificada")
                .Select(r => new
                {
                    r.Id,
                    Usuario = r.Usuario.Nombre + " " + r.Usuario.Apellido,
                    Libro = r.Libro.Titulo,
                    r.Prioridad,
                    r.FechaNotificacion,
                    r.FechaExpiracion,
                    Expirada = r.FechaExpiracion < DateTime.UtcNow
                })
                .OrderByDescending(r => r.FechaNotificacion)
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/reportes/libros-mas-prestados (Top 10)
        [HttpGet("libros-mas-prestados")]
        public async Task<IActionResult> LibrosMasPrestados()
        {
            var topLibros = await _context.Prestamos
                .Include(p => p.Libro)
                .GroupBy(p => p.Libro)
                .Select(g => new
                {
                    Libro = g.Key.Titulo,
                    Autor = g.Key.Autor,        // ajusta si tienes campo Autor
                    TotalPrestamos = g.Count()
                })
                .OrderByDescending(x => x.TotalPrestamos)
                .Take(10)
                .ToListAsync();

            return Ok(topLibros);
        }
    }
}
