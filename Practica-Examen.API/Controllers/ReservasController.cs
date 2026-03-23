namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.Models;
using Practica_Examen.API.Services;
[ApiController]
[Route("api/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservasService _reservasService;
    private readonly ILibrosService _librosService;

    public ReservasController(IReservasService reservasService, ILibrosService librosService)
    {
        _reservasService = reservasService;
        _librosService = librosService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Reserva req)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();

        var libro = await _librosService.GetByIdAsync(req.LibroId);
        if (libro == null) return NotFound(new { message = "Libro no encontrado" });
        if (libro.Disponible)
            return BadRequest(new { message = "El libro está disponible, puede solicitarlo en préstamo" });

        // Verificar reserva existente
        var reservasUsuario = await _reservasService.GetByUsuarioIdAsync(userId);
        if (reservasUsuario.Any(r => r.LibroId == req.LibroId && r.Estado == "activa"))
            return BadRequest(new { message = "Ya tiene una reserva activa para este libro" });

        var reserva = new Reserva
        {
            UsuarioId = userId,
            LibroId = req.LibroId,
            FechaReserva = DateTime.UtcNow,
            Estado = "activa"
        };

        var creada = await _reservasService.CreateAsync(reserva);
        return Ok(new { message = "Reserva creada exitosamente", data = creada });
    }

    [HttpGet("mis-reservas")]
    public async Task<IActionResult> MisReservas()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();

        var reservas = await _reservasService.GetByUsuarioIdAsync(userId);
        return Ok(reservas.OrderByDescending(r => r.FechaReserva));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancelar(string id)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();

        var reserva = await _reservasService.GetByIdAsync(id);
        if (reserva == null) return NotFound(new { message = "Reserva no encontrada" });
        if (reserva.UsuarioId != userId)
            return Forbid();
        if (reserva.Estado != "activa")
            return BadRequest(new { message = "Solo se pueden cancelar reservas activas" });

        await _reservasService.DeleteAsync(id);
        return Ok(new { message = "Reserva cancelada exitosamente" });
    }
}