namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.Models;
using Practica_Examen.API.Services;
using System.Security.Claims;
[ApiController]
[Route("api/prestamos")]
[Authorize]
public class PrestamosController : ControllerBase
{
    private readonly IPrestamosService _prestamosService;
    private readonly ILibrosService _librosService;
    private readonly IUsuariosService _usuariosService;

    public PrestamosController(IPrestamosService prestamosService,
        ILibrosService librosService, IUsuariosService usuariosService)
    {
        _prestamosService = prestamosService;
        _librosService = librosService;
        _usuariosService = usuariosService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Prestamo req)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();

        var libro = await _librosService.GetByIdAsync(req.LibroId);
        if (libro == null) return NotFound(new { message = "Libro no encontrado" });
        if (!libro.Disponible || libro.Stock <= 0)
            return BadRequest(new { message = "El libro no tiene copias disponibles" });

        var usuario = await _usuariosService.GetByIdAsync(userId);
        if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

        // Verificar préstamos activos
        var prestamosActivos = await _prestamosService.GetByUsuarioIdAsync(userId);
        if (prestamosActivos.Count(p => p.Estado == "activo") >= 3)
            return BadRequest(new { message = "No puede tener más de 3 préstamos activos" });

        // Crear préstamo
        var prestamo = new Prestamo
        {
            UsuarioId = userId,
            LibroId = req.LibroId,
            FechaPrestamo = DateTime.UtcNow,
            FechaDevolucion = DateTime.UtcNow.AddDays(14),
            Estado = "activo"
        };

        // Reducir stock
        libro.Stock--;
        libro.Disponible = libro.Stock > 0;
        await _librosService.UpdateAsync(libro.Id!, libro);

        var creado = await _prestamosService.CreateAsync(prestamo);
        return Ok(new { message = "Préstamo creado exitosamente", data = creado });
    }

    [HttpGet("mis-prestamos")]
    public async Task<IActionResult> MisPrestamos()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();

        var prestamos = await _prestamosService.GetByUsuarioIdAsync(userId);
        return Ok(prestamos);
    }

    [HttpPut("{id}/devolver")]
    public async Task<IActionResult> Devolver(string id)
    {
        var prestamo = await _prestamosService.GetByIdAsync(id);
        if (prestamo == null) return NotFound(new { message = "Préstamo no encontrado" });
        if (prestamo.Estado != "activo")
            return BadRequest(new { message = "Este préstamo ya fue devuelto" });

        var ahora = DateTime.UtcNow;
        prestamo.FechaDevolucion = ahora;
        prestamo.Estado = "devuelto";

        // Calcular multa si hay retraso
        if (prestamo.FechaDevolucion > ahora)
        {
            var diasRetraso = (int)(ahora - prestamo.FechaDevolucion.Value).TotalDays;
            if (diasRetraso > 0)
            {
                var multa = diasRetraso * 50.0;
                prestamo.Estado = "devuelto";
            }
        }

        await _prestamosService.UpdateAsync(id, prestamo);

        // Incrementar stock del libro
        var libro = await _librosService.GetByIdAsync(prestamo.LibroId);
        if (libro != null)
        {
            libro.Stock++;
            libro.Disponible = true;
            await _librosService.UpdateAsync(libro.Id!, libro);
        }

        return Ok(new { message = "Libro devuelto exitosamente" });
    }

    [HttpGet("vencidos")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> Vencidos()
    {
        var todos = await _prestamosService.GetAllAsync();
        var vencidos = todos.Where(p =>
            p.Estado == "activo" &&
            p.FechaDevolucion.HasValue &&
            p.FechaDevolucion.Value < DateTime.UtcNow).ToList();
        return Ok(vencidos);
    }
}