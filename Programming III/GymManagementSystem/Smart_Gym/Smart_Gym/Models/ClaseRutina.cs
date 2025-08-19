using Smart_Gym.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class ClaseRutina
    {
        [Key]
        public int IdClaseRutina { get; set; }

        // Foreign key
        public int IdClase { get; set; }
        public int IdRutina { get; set; }

        //Navegación
        [ForeignKey("IdClase")]
        [Display(Name = "Clase")]
        public Clase Clase { get; set; }

        [ForeignKey("IdRutina")]
        [Display(Name = "Rutina")]
        public required Rutina Rutina { get; set; }

        public ICollection<UsuarioClase> UsuarioClases { get; set; } = new List<UsuarioClase>();

    }
}
