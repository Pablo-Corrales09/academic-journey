using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class UsuarioClase
    {
        [Key]
        public int IdUsuarioClase { get; set; }

        // Llave foranea
        public string IdUsuario { get; set; }

        public int IdClaseRutina { get; set; }

        // Navegación
        [ForeignKey("IdUsuario")]
        [Display(Name = "Usuario")]
        public Usuario Usuario { get; set; }

        [ForeignKey("IdClaseRutina")]
        [Display(Name = "Clase y rutina")]
        public ClaseRutina ClaseRutina { get; set; }
    }

}
