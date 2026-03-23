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
    public class ReservasController : ControllerBase
    {
        private readonly FirestoreService _firestore;

        public ReservasController(FirestoreService firestore)
        {
            _firestore = firestore;
        }

        // POST /api/reservas
        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] CrearReservaDto dto)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("Usuario no autenticado.");

            // Verificar que el libro exista y NO tenga copias disponibles
            var libroDoc = await _firestore.Libros.Document(dto.LibroId).GetSnapshotAsync();
            if (!libroDoc.Exists)
                return BadRequest("El libro no existe.");

            var libro = libroDoc.ConvertTo<Libro>();
            if (libro.CopiasDisponibles != 0)
                return BadRequest("El libro debe tener 0 copias disponibles para poder reservarse.");

            // Verificar que el usuario no tenga ya una reserva pendiente o notificada para este libro
            var yaTieneReservaQuery = _firestore.Reservas
                .WhereEqualTo("usuarioId", usuarioId)
                .WhereEqualTo("libroId", dto.LibroId)
                .WhereIn("estado", new[] { "pendiente", "notificada" });

            var yaTieneSnapshot = await yaTieneReservaQuery.GetSnapshotAsync();
            if (yaTieneSnapshot.Documents.Any())
                return BadRequest("Ya tiene una reserva pendiente o notificada para este libro.");

            // Obtener el total de reservas pendientes para calcular prioridad
            var totalPendientesQuery = _firestore.Reservas
                .WhereEqualTo("libroId", dto.LibroId)
                .WhereEqualTo("estado", "pendiente");

            var totalSnapshot = await totalPendientesQuery.GetSnapshotAsync();
            int prioridad = totalSnapshot.Documents.Count() + 1;

            // Crear la reserva
            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                LibroId = dto.LibroId,
                Prioridad = prioridad,
                Estado = "pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            var docRef = await _firestore.Reservas.AddAsync(reserva);
            reserva.Id = docRef.Id;

            return CreatedAtAction(nameof(CrearReserva), new { id = reserva.Id }, reserva);
        }

        // GET /api/reservas/mis-reservas
        [HttpGet("mis-reservas")]
        public async Task<IActionResult> MisReservas()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

            var query = _firestore.Reservas
                .WhereEqualTo("usuarioId", usuarioId)
                .OrderBy("fechaCreacion");

            var snapshot = await query.GetSnapshotAsync();

            var reservas = snapshot.Documents.Select(doc =>
            {
                var r = doc.ConvertTo<Reserva>();
                r.Id = doc.Id;

                // Obtener info básica del libro (puedes cachear o usar un join manual si lo prefieres)
                var libroTask = _firestore.Libros.Document(r.LibroId).GetSnapshotAsync();
                var libroSnap = libroTask.Result; // bloqueante solo para simplicidad (en producción usa Task.WhenAll)

                var libro = libroSnap.Exists ? libroSnap.ConvertTo<Libro>() : new Libro { Titulo = "Desconocido", Autor = "" };

                return new
                {
                    Id = r.Id,
                    Libro = new { Titulo = libro.Titulo, Autor = libro.Autor },
                    r.Estado,
                    r.Prioridad,
                    PosicionEnCola = r.Prioridad,
                    r.FechaCreacion,
                    r.FechaNotificacion,
                    r.FechaExpiracion
                };
            }).ToList();

            return Ok(reservas);
        }

        // DELETE /api/reservas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarReserva(string id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

            var docRef = _firestore.Reservas.Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound("Reserva no encontrada.");

            var reserva = snapshot.ConvertTo<Reserva>();
            if (reserva.UsuarioId != usuarioId)
                return Forbid("No puedes cancelar una reserva que no te pertenece.");

            if (reserva.Estado != "pendiente" && reserva.Estado != "notificada")
                return BadRequest("Solo se pueden cancelar reservas en estado pendiente o notificada.");

            var libroId = reserva.LibroId;

            // Eliminar la reserva
            await docRef.DeleteAsync();

            // Recalcular prioridades de las reservas pendientes restantes para ese libro
            var pendientesQuery = _firestore.Reservas
                .WhereEqualTo("libroId", libroId)
                .WhereEqualTo("estado", "pendiente")
                .OrderBy("prioridad");

            var pendientesSnapshot = await pendientesQuery.GetSnapshotAsync();

            var batch = _firestore._db.StartBatch(); // Nota: usa el _db interno o agrega método en FirestoreService

            int nuevaPrioridad = 1;
            foreach (var doc in pendientesSnapshot.Documents)
            {
                var updateData = new Dictionary<string, object>
                {
                    { "prioridad", nuevaPrioridad }
                };
                batch.Update(doc.Reference, updateData);
                nuevaPrioridad++;
            }

            await batch.CommitAsync();

            return Ok(new { mensaje = "Reserva cancelada y prioridades actualizadas correctamente." });
        }
    }

    public class CrearReservaDto
    {
        public string LibroId { get; set; } = string.Empty;   // Cambiado a string porque Firestore usa string Id
    }
}
