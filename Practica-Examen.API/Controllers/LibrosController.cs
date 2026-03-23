namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.Models;
using Practica_Examen.API.Services;
[ApiController]
[Route("api/libros")]
[Authorize]
public class LibrosController : ControllerBase
{
    private readonly ILibrosService _librosService;

    public LibrosController(ILibrosService librosService)
    {
        _librosService = librosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? categoria,
        [FromQuery] string? autor,
        [FromQuery] bool? disponible)
    {
        var libros = await _librosService.GetAllAsync();

        if (!string.IsNullOrEmpty(categoria))
            libros = libros.Where(l => l.Categoria == categoria).ToList();

        if (!string.IsNullOrEmpty(autor))
            libros = libros.Where(l => l.Autor == autor).ToList();

        if (disponible == true)
            libros = libros.Where(l => l.Disponible).ToList();

        return Ok(libros);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var libro = await _librosService.GetByIdAsync(id);
        if (libro == null) return NotFound(new { message = "Libro no encontrado" });
        return Ok(libro);
    }

    [HttpPost]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> Create([FromBody] Libro libro)
    {
        // Validar ISBN único
        var todos = await _librosService.GetAllAsync();
        if (todos.Any(l => l.ISBN == libro.ISBN))
            return BadRequest(new { message = "Ya existe un libro con ese ISBN" });

        if (libro.Stock < 0)
            return BadRequest(new { message = "El stock no puede ser negativo" });

        libro.Disponible = libro.Stock > 0;
        var creado = await _librosService.CreateAsync(libro);
        return Ok(new { message = "Libro creado exitosamente", data = creado });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> Update(string id, [FromBody] Libro libro)
    {
        var existente = await _librosService.GetByIdAsync(id);
        if (existente == null) return NotFound(new { message = "Libro no encontrado" });

        libro.Id = id;
        libro.Disponible = libro.Stock > 0;
        var actualizado = await _librosService.UpdateAsync(id, libro);
        return Ok(new { message = "Libro actualizado exitosamente", data = actualizado });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var existente = await _librosService.GetByIdAsync(id);
        if (existente == null) return NotFound(new { message = "Libro no encontrado" });

        await _librosService.DeleteAsync(id);
        return Ok(new { message = "Libro eliminado exitosamente" });
    }
}