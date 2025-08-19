using System.ComponentModel.DataAnnotations;

namespace Smart_Gym.Models
{
    public class Rutina
    {
        [Key]
        public int IdRutina { get; set; }

        [Required(ErrorMessage = "El campo debe agregar un nombre para la rutina.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "El nombre de la rutina debe contener entre 1 a 40 caracteres.")]
        [Display(Name = "Nombre")]
        public string NombreRutina { get; set; }

        [Required(ErrorMessage = "El campo debe agregar un nivel para la rutina.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "El nivel debe contener entre 1 a 40 caracteres.")]
        public string Nivel { get; set; }

        [Required(ErrorMessage = "El campo debe agregar una descripción de la rutina.")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "La descripción de la rutina debe contener entre 1 a 250 caracteres.")]
        public string DescripcionRutina { get; set; }

        //Navegación 
        public ICollection<EjercicioRutina> EjerciciosRutina { get; set; }

        public ICollection<ClaseRutina> ClaseRutina { get; set; } = new List<ClaseRutina>();

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();


    }//Fin Clase Rutina
}
