using System.ComponentModel.DataAnnotations;

namespace Practica_Examen.API.Models
{
    public class PrestamoDto
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public int LibroId { get; set; }
        public Libro Libro { get; set; }

        public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;
        public DateTime FechaDevolucionEsperada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }

        [Required]
        public string Estado { get; set; } = "activo"; // "activo" o "devuelto"
    }
}
