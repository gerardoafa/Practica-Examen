namespace Practica_Examen.API.Services;

public interface IReportesService
{
    Task<int> GetTotalLibrosAsync();
    Task<int> GetTotalPrestamosAsync();
    Task<int> GetTotalReservasActivasAsync();
    Task<Dictionary<string, int>> GetPrestamosPorEstadoAsync();
}
