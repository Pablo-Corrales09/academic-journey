using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class Clase
    {
        [Key]
        public int IdClase { get; set; }

        [Required(ErrorMessage = "Debe agregar el tipo de clase.")]
        [StringLength(50, ErrorMessage = "El tipo de rutina no puede exceder los 50 caracteres.")]
        public required string Tipo { get; set; } // "Personal" o "Grupal"

        [Required(ErrorMessage = "Debe agregar nombre a la clase.")]
        [StringLength(100, ErrorMessage = "El nombre de la clase no puede exceder los 100 caracteres.")]
        public required string Nombre { get; set; }

        public DateTime FechaHora { get; set; }

        //llave Foranea
        public int IdGimnasio { get; set; }

        //Navegación
        [ForeignKey("IdGimnasio")]
        public Gimnasio Gimnasio { get; set; }

        public ICollection<UsuarioClase> Usuarios { get; set; } = new List<UsuarioClase>();

        public ICollection<ClaseRutina> Rutinas { get; set; } = new List<ClaseRutina>();

    }
}
