using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.DTOs;
using Practica_Examen.API.Models;
using Practica_Examen.API.Services;

namespace Practica_Examen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LibrosController : ControllerBase
{
    private readonly FirestoreService _firestore;

    public LibrosController(FirestoreService firestore)
    {
        _firestore = firestore;
    }

    // GET: api/libros
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibroDto>>> GetLibros()
    {
        var snapshot = await _firestore.Libros.GetSnapshotAsync();
        var libros = snapshot.Documents.Select(doc =>
        {
            var libro = doc.ConvertTo<Libro>();
            libro.Id = doc.Id;
            return new LibroDto
            {
                Id = libro.Id,
                Titulo = libro.Titulo,
                Autor = libro.Autor,
                ISBN = libro.ISBN,
                Categoria = libro.Categoria,
                Stock = libro.CopiasDisponibles,
                Disponible = libro.CopiasDisponibles > 0
            };
        }).ToList();

        return Ok(libros);
    }

    // GET: api/libros/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<LibroDto>> GetLibro(string id)
    {
        var docRef = _firestore.Libros.Document(id);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return NotFound("Libro no encontrado.");

        var libro = snapshot.ConvertTo<Libro>();
        libro.Id = snapshot.Id;

        var dto = new LibroDto
        {
            Id = libro.Id,
            Titulo = libro.Titulo,
            Autor = libro.Autor,
            ISBN = libro.ISBN,
            Categoria = libro.Categoria,
            Stock = libro.CopiasDisponibles,
            Disponible = libro.CopiasDisponibles > 0
        };

        return Ok(dto);
    }

    // POST: api/libros (Solo Admin)
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

        var docRef = await _firestore.Libros.AddAsync(libro);
        libro.Id = docRef.Id;

        var libroDto = new LibroDto
        {
            Id = libro.Id,
            Titulo = libro.Titulo,
            Autor = libro.Autor,
            ISBN = libro.ISBN,
            Categoria = libro.Categoria,
            Stock = libro.CopiasDisponibles,
            Disponible = libro.CopiasDisponibles > 0
        };

        return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libroDto);
    }

    // PUT: api/libros/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarLibro(string id, [FromBody] CrearLibroDto dto)
    {
        var docRef = _firestore.Libros.Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return NotFound();

        var updates = new Dictionary<string, object>
        {
            { "titulo", dto.Titulo },
            { "autor", dto.Autor },
            { "isbn", dto.ISBN },
            { "categoria", dto.Categoria },
            { "copiasDisponibles", dto.Stock }
        };

        await docRef.UpdateAsync(updates);
        return NoContent();
    }

    // DELETE: api/libros/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarLibro(string id)
    {
        // Verificar si tiene préstamos activos o reservas (Firestore no tiene joins fáciles, así que consultamos)
        var prestamosActivos = await _firestore.Prestamos
            .WhereEqualTo("libroId", id)
            .WhereEqualTo("estado", "activo")
            .GetSnapshotAsync();

        var reservas = await _firestore.Reservas
            .WhereEqualTo("libroId", id)
            .GetSnapshotAsync();

        if (prestamosActivos.Documents.Any() || reservas.Documents.Any())
            return BadRequest("No se puede eliminar el libro porque tiene préstamos o reservas.");

        await _firestore.Libros.Document(id).DeleteAsync();
        return NoContent();
    }
}

public class CrearLibroDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Stock { get; set; }
}
