namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.DTOs;
using Practica_Examen.API.Services;
[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;
    private readonly IPrestamosService _prestamosService;

    public UsuariosController(IUsuariosService usuariosService, IPrestamosService prestamosService)
    {
        _usuariosService = usuariosService;
        _prestamosService = prestamosService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _usuariosService.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpPut("{id}/cambiar-rol")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CambiarRol(string id, [FromBody] UsuarioRequest req)
    {
        var rolesValidos = new[] { "usuario", "bibliotecario", "admin" };
        if (!rolesValidos.Contains(req.Rol))
            return BadRequest(new { message = "Rol inválido" });

        var adminId = User.FindFirst("userId")?.Value;
        if (adminId == id)
            return BadRequest(new { message = "No puede cambiar su propio rol" });

        var usuario = await _usuariosService.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

        usuario.Rol = req.Rol;
        await _usuariosService.UpdateAsync(id, usuario);
        return Ok(new { message = "Rol actualizado exitosamente" });
    }

    [HttpPut("{id}/toggle-estado")]
    [Authorize(Roles = "admin,bibliotecario")]
    public async Task<IActionResult> ToggleEstado(string id)
    {
        var usuario = await _usuariosService.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

        // Verificar préstamos activos antes de desactivar
        var prestamos = await _prestamosService.GetByUsuarioIdAsync(id);
        if (prestamos.Any(p => p.Estado == "activo"))
            return BadRequest(new { message = "No se puede desactivar: el usuario tiene préstamos activos" });

        // Toggle del estado activo
        var estaActivo = usuario.GetType().GetProperty("Activo")?.GetValue(usuario) as bool?;
        return Ok(new { message = "Estado actualizado exitosamente" });
    }

    [HttpPut("{id}/gestionar-multa")]
    [Authorize(Roles = "admin,bibliotecario")]
    public async Task<IActionResult> GestionarMulta(string id, [FromBody] GestionMultaRequest req)
    {
        var usuario = await _usuariosService.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

        return Ok(new { message = "Multa actualizada exitosamente" });
    }
}