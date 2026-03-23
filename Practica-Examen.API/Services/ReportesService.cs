namespace Practica_Examen.API.Services;

public class ReportesService : IReportesService
{
    private readonly FirebaseService _firebaseService;
    private readonly ILibrosService _librosService;
    private readonly IPrestamosService _prestamosService;
    private readonly IReservasService _reservasService;

    public ReportesService(
        FirebaseService firebaseService,
        ILibrosService librosService,
        IPrestamosService prestamosService,
        IReservasService reservasService)
    {
        _firebaseService = firebaseService;
        _librosService = librosService;
        _prestamosService = prestamosService;
        _reservasService = reservasService;
    }

    public async Task<int> GetTotalLibrosAsync()
    {
        var libros = await _librosService.GetAllAsync();
        return libros.Count;
    }

    public async Task<int> GetTotalPrestamosAsync()
    {
        var prestamos = await _prestamosService.GetAllAsync();
        return prestamos.Count;
    }

    public async Task<int> GetTotalReservasActivasAsync()
    {
        var reservas = await _reservasService.GetAllAsync();
        return reservas.Count(r => r.Estado == "activa");
    }

    public async Task<Dictionary<string, int>> GetPrestamosPorEstadoAsync()
    {
        var prestamos = await _prestamosService.GetAllAsync();
        return prestamos
            .GroupBy(p => p.Estado)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}