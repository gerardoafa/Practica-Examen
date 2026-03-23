using System.ComponentModel.DataAnnotations;

namespace Practica_Examen.API.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public int LibroId { get; set; }
        public Libro Libro { get; set; }

        [Required]
        public int Prioridad { get; set; }

        [Required]
        public string Estado { get; set; } = "pendiente"; // "pendiente" o "notificada"

        public DateTime? FechaNotificacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
