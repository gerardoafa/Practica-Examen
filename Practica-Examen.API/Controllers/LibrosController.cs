using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Practica_Examen.API.Data;
using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;

namespace Practica_Examen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibrosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/libros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibroDto>>> GetLibros()
        {
            var libros = await _context.Libros
                .Select(l => new LibroDto
                {
                    Id = l.Id.ToString(),
                    Titulo = l.Titulo,
                    Autor = l.Autor,
                    ISBN = l.ISBN,
                    Categoria = l.Categoria,
                    Stock = l.CopiasDisponibles,           // Usamos CopiasDisponibles como Stock
                    Disponible = l.CopiasDisponibles > 0
                })
                .ToListAsync();

            return Ok(libros);
        }

        // GET: api/libros/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LibroDto>> GetLibro(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
                return NotFound("Libro no encontrado.");

            var libroDto = new LibroDto
            {
                Id = libro.Id.ToString(),
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                ISBN = libro.ISBN,
                Categoria = libro.Categoria,
                Stock = libro.CopiasDisponibles,
                Disponible = libro.CopiasDisponibles > 0
            };

            return Ok(libroDto);
        }

        // POST: api/libros  (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LibroDto>> CrearLibro([FromBody] CrearLibroDto dto)
        {
            var libro = new Libro
            {
                Titulo = dto.Titulo,
                Autor = dto.Autor,
                ISBN = dto.ISBN,
                Categoria = dto.Categoria,
                CopiasDisponibles = dto.Stock
            };

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            var libroDto = new LibroDto
            {
                Id = libro.Id.ToString(),
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                ISBN = libro.ISBN,
                Categoria = libro.Categoria,
                Stock = libro.CopiasDisponibles,
                Disponible = libro.CopiasDisponibles > 0
            };

            return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libroDto);
        }

        // PUT: api/libros/{id}  (Solo Admin - Actualizar libro)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarLibro(int id, [FromBody] CrearLibroDto dto)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
                return NotFound("Libro no encontrado.");

            libro.Titulo = dto.Titulo;
            libro.Autor = dto.Autor;
            libro.ISBN = dto.ISBN;
            libro.Categoria = dto.Categoria;
            libro.CopiasDisponibles = dto.Stock;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/libros/{id}  (Solo Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarLibro(int id)
        {
            var libro = await _context.Libros
                .Include(l => l.Prestamos)
                .Include(l => l.Reservas)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null)
                return NotFound("Libro no encontrado.");

            // No permitir eliminar si tiene préstamos activos o reservas
            if (libro.Prestamos.Any(p => p.Estado == "activo") || libro.Reservas.Any())
            {
                return BadRequest("No se puede eliminar el libro porque tiene préstamos activos o reservas pendientes.");
            }

            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/libros/buscar?termino=...
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<LibroDto>>> BuscarLibros([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await GetLibros();

            var libros = await _context.Libros
                .Where(l => l.Titulo.Contains(termino) ||
                            l.Autor.Contains(termino) ||
                            l.ISBN.Contains(termino) ||
                            l.Categoria.Contains(termino))
                .Select(l => new LibroDto
                {
                    Id = l.Id.ToString(),
                    Titulo = l.Titulo,
                    Autor = l.Autor,
                    ISBN = l.ISBN,
                    Categoria = l.Categoria,
                    Stock = l.CopiasDisponibles,
                    Disponible = l.CopiasDisponibles > 0
                })
                .ToListAsync();

            return Ok(libros);
        }
    }

    // DTO para Crear y Actualizar libro
    public class CrearLibroDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
