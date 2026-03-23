namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.Services;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "bibliotecario,admin")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;

    public ReportesController(IReportesService reportesService)
    {
        _reportesService = reportesService;
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> Estadisticas()
    {
        var totalLibros = await _reportesService.GetTotalLibrosAsync();
        var totalPrestamos = await _reportesService.GetTotalPrestamosAsync();
        var reservasActivas = await _reportesService.GetTotalReservasActivasAsync();
        var prestamosPorEstado = await _reportesService.GetPrestamosPorEstadoAsync();

        return Ok(new
        {
            totalLibros,
            totalPrestamos,
            reservasActivas,
            prestamosPorEstado
        });
    }

    [HttpGet("mi-historial")]
    [Authorize]
    public async Task<IActionResult> MiHistorial()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) return Unauthorized();
        return Ok(new { message = "Historial", userId });
    }
}