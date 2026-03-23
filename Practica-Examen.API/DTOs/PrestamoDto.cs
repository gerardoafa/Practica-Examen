namespace Practica_Examen.API.DTOs;

public class PrestamoDto
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string LibroId { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
