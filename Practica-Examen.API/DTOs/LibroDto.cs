namespace Practica_Examen.API.DTOs;

public class LibroDto
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool Disponible { get; set; }
}
