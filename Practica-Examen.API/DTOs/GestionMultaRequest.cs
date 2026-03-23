namespace Practica_Examen.API.DTOs;

public class GestionMultaRequest
{
    public string PrestamoId { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
}