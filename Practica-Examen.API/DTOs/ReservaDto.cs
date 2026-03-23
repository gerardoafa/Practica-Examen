namespace Practica_Examen.API.DTOs;

public class ReservaRequest
{
    public string UsuarioId { get; set; } = string.Empty;
    public string LibroId { get; set; } = string.Empty;
}

public class ReservaDto
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string LibroId { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public string Estado { get; set; } = string.Empty;
}
