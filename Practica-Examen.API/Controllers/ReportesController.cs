using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.Models;
using Practica_Examen.API.Services;
using System.Security.Claims;

namespace Practica_Examen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly FirestoreService _firestore;

        public ReportesController(FirestoreService firestore)
        {
            _firestore = firestore;
        }

        // GET: api/reportes/prestamos-activos
        [HttpGet("prestamos-activos")]
        public async Task<IActionResult> PrestamosActivos()
        {
            var snapshot = await _firestore.Prestamos
                .WhereEqualTo("estado", "activo")
                .OrderByDescending("fechaPrestamo")
                .GetSnapshotAsync();

            var prestamos = new List<object>();

            foreach (var doc in snapshot.Documents)
            {
                var p = doc.ConvertTo<Prestamo>();
                p.Id = doc.Id;

                var usuarioSnap = await _firestore.Usuarios.Document(p.UsuarioId).GetSnapshotAsync();
                var libroSnap = await _firestore.Libros.Document(p.LibroId).GetSnapshotAsync();

                var usuario = usuarioSnap.Exists ? usuarioSnap.ConvertTo<Usuario>() : new Usuario { Nombre = "Desconocido", Apellido = "" };
                var libro = libroSnap.Exists ? libroSnap.ConvertTo<Libro>() : new Libro { Titulo = "Desconocido" };

                prestamos.Add(new
                {
                    Id = p.Id,
                    Usuario = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
                    Libro = libro.Titulo,
                    p.FechaPrestamo,
                    p.FechaDevolucionEsperada,
                    DiasRestantes = (p.FechaDevolucionEsperada - DateTime.UtcNow).Days
                });
            }

            return Ok(prestamos);
        }

        // GET: api/reportes/prestamos-vencidos
        [HttpGet("prestamos-vencidos")]
        public async Task<IActionResult> PrestamosVencidos()
        {
            var hoy = DateTime.UtcNow;

            var snapshot = await _firestore.Prestamos
                .WhereEqualTo("estado", "activo")
                .GetSnapshotAsync(); // Firestore no permite < en la misma query fácilmente, filtramos en memoria

            var vencidos = new List<object>();

            foreach (var doc in snapshot.Documents)
            {
                var p = doc.ConvertTo<Prestamo>();
                p.Id = doc.Id;

                if (p.FechaDevolucionEsperada >= hoy) continue;

                var usuarioSnap = await _firestore.Usuarios.Document(p.UsuarioId).GetSnapshotAsync();
                var libroSnap = await _firestore.Libros.Document(p.LibroId).GetSnapshotAsync();

                var usuario = usuarioSnap.Exists ? usuarioSnap.ConvertTo<Usuario>() : new Usuario { Nombre = "Desconocido", Apellido = "" };
                var libro = libroSnap.Exists ? libroSnap.ConvertTo<Libro>() : new Libro { Titulo = "Desconocido" };

                var diasRetraso = (hoy - p.FechaDevolucionEsperada).Days;

                vencidos.Add(new
                {
                    Id = p.Id,
                    Usuario = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
                    Libro = libro.Titulo,
                    p.FechaPrestamo,
                    p.FechaDevolucionEsperada,
                    DiasRetraso = diasRetraso,
                    MultaEstimada = diasRetraso * 50m
                });
            }

            // Ordenar en memoria
            var resultado = vencidos.OrderByDescending(x => ((dynamic)x).DiasRetraso).ToList();

            return Ok(resultado);
        }

        // GET: api/reportes/multas-usuarios
        [HttpGet("multas-usuarios")]
        public async Task<IActionResult> MultasPorUsuario()
        {
            var usuariosSnapshot = await _firestore.Usuarios
                .WhereGreaterThan("multas", 0)
                .GetSnapshotAsync();

            var resultado = new List<object>();

            foreach (var doc in usuariosSnapshot.Documents)
            {
                var u = doc.ConvertTo<Usuario>();
                u.Id = doc.Id;

                // Contar préstamos activos del usuario
                var prestamosActivosSnap = await _firestore.Prestamos
                    .WhereEqualTo("usuarioId", u.Id)
                    .WhereEqualTo("estado", "activo")
                    .GetSnapshotAsync();

                resultado.Add(new
                {
                    Id = u.Id,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}".Trim(),
                    Multas = u.Multas,
                    PrestamosActivos = prestamosActivosSnap.Documents.Count
                });
            }

            var ordenado = resultado.OrderByDescending(x => ((dynamic)x).Multas).ToList();
            return Ok(ordenado);
        }

        // GET: api/reportes/reservas-pendientes
        [HttpGet("reservas-pendientes")]
        public async Task<IActionResult> ReservasPendientes()
        {
            var snapshot = await _firestore.Reservas
                .WhereEqualTo("estado", "pendiente")
                .OrderBy("fechaCreacion")
                .GetSnapshotAsync();

            var reservas = await ProcesarReservasConInfo(snapshot);
            return Ok(reservas);
        }

        // GET: api/reportes/reservas-notificadas
        [HttpGet("reservas-notificadas")]
        public async Task<IActionResult> ReservasNotificadas()
        {
            var snapshot = await _firestore.Reservas
                .WhereEqualTo("estado", "notificada")
                .OrderByDescending("fechaNotificacion")
                .GetSnapshotAsync();

            var reservas = await ProcesarReservasConInfo(snapshot);
            return Ok(reservas);
        }

        // GET: api/reportes/libros-mas-prestados (Top 10)
        [HttpGet("libros-mas-prestados")]
        public async Task<IActionResult> LibrosMasPrestados()
        {
            var snapshot = await _firestore.Prestamos.GetSnapshotAsync();

            var conteo = new Dictionary<string, (string Titulo, string Autor, int Total)>();

            foreach (var doc in snapshot.Documents)
            {
                var p = doc.ConvertTo<Prestamo>();
                var libroSnap = await _firestore.Libros.Document(p.LibroId).GetSnapshotAsync();

                if (!libroSnap.Exists) continue;

                var libro = libroSnap.ConvertTo<Libro>();

                if (conteo.ContainsKey(p.LibroId))
                {
                    var actual = conteo[p.LibroId];
                    conteo[p.LibroId] = (actual.Titulo, actual.Autor, actual.Total + 1);
                }
                else
                {
                    conteo[p.LibroId] = (libro.Titulo, libro.Autor, 1);
                }
            }

            var top10 = conteo.Values
                .OrderByDescending(x => x.Total)
                .Take(10)
                .Select(x => new
                {
                    Libro = x.Titulo,
                    Autor = x.Autor,
                    TotalPrestamos = x.Total
                })
                .ToList();

            return Ok(top10);
        }

        // Método helper privado para evitar repetir código
        private async Task<List<object>> ProcesarReservasConInfo(QuerySnapshot snapshot)
        {
            var lista = new List<object>();

            foreach (var doc in snapshot.Documents)
            {
                var r = doc.ConvertTo<Reserva>();
                r.Id = doc.Id;

                var usuarioSnap = await _firestore.Usuarios.Document(r.UsuarioId).GetSnapshotAsync();
                var libroSnap = await _firestore.Libros.Document(r.LibroId).GetSnapshotAsync();

                var usuario = usuarioSnap.Exists ? usuarioSnap.ConvertTo<Usuario>() : new Usuario { Nombre = "Desconocido", Apellido = "" };
                var libro = libroSnap.Exists ? libroSnap.ConvertTo<Libro>() : new Libro { Titulo = "Desconocido" };

                lista.Add(new
                {
                    Id = r.Id,
                    Usuario = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
                    Libro = libro.Titulo,
                    r.Prioridad,
                    r.FechaCreacion,
                    r.FechaNotificacion,
                    r.FechaExpiracion,
                    Expirada = r.FechaExpiracion.HasValue && r.FechaExpiracion < DateTime.UtcNow
                });
            }

            return lista;
        }
    }
}
